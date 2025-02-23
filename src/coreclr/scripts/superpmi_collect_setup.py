#!/usr/bin/env python3
#
# Licensed to the .NET Foundation under one or more agreements.
# The .NET Foundation licenses this file to you under the MIT license.
#
# Title               : superpmi_collect_setup.py
#
# Notes:
#
# Script to setup directory structure required to perform SuperPMI collection in CI.
# It does the following steps:
# 1.  It creates `correlation_payload_directory` that contains files from CORE_ROOT, src\coreclr\scripts.
#     This directory is the one that is sent to all the helix machines that performs SPMI collection.
# 2.  It clones dotnet/jitutils, builds it and then copies the `pmi.dll` to `correlation_payload_directory` folder.
#     This file is needed to do pmi SPMI runs.
# 3.  The script takes `input_artifacts` parameter which contains managed .dlls and .exes on
#     which SPMI needs to be run. This script will partition these folders into equal buckets of approximately `max_size`
#     bytes and stores them under `payload` directory. Each sub-folder inside `payload` directory is sent to individual
#     helix machine to do SPMI collection on. E.g. for `input_artifacts` to be run on libraries, the parameter would be path to
#     `CORE_ROOT` folder and this script will copy `max_size` bytes of those files under `payload/libraries/0/binaries`,
#     `payload/libraries/1/binaries` and so forth.
# 4.  Lastly, it sets the pipeline variables.
#
# Below are the helix queues it sets depending on the OS/architecture:
# | Arch  | windows                 | Linux                                                                                                                                | macOS          |
# |-------|-------------------------|--------------------------------------------------------------------------------------------------------------------------------------|----------------|
# | x86   | Windows.10.Amd64.X86.Rt |                                                                                                                                      | -              |
# | x64   | Windows.10.Amd64.X86.Rt | Ubuntu.1804.Amd64                                                                                                                    | OSX.1014.Amd64 |
# | arm   | -                       | (Ubuntu.1804.Arm32)Ubuntu.1804.Armarch@mcr.microsoft.com/dotnet-buildtools/prereqs:ubuntu-18.04-helix-arm32v7-bfcd90a-20200121150440 | -              |
# | arm64 | Windows.10.Arm64        | (Ubuntu.1804.Arm64)Ubuntu.1804.ArmArch@mcr.microsoft.com/dotnet-buildtools/prereqs:ubuntu-18.04-helix-arm64v8-20210531091519-97d8652 | OSX.1100.ARM64 |
#
################################################################################
################################################################################

import argparse
import os
import stat

from coreclr_arguments import *
from jitutil import run_command, copy_directory, copy_files, set_pipeline_variable, ChangeDir, TempDir


# Start of parser object creation.

parser = argparse.ArgumentParser(description="description")

parser.add_argument("-source_directory", help="path to source directory")
parser.add_argument("-core_root_directory", help="path to core_root directory")
parser.add_argument("-arch", help="Architecture")
parser.add_argument("-platform", help="OS platform")
parser.add_argument("-mch_file_tag", help="Tag to be used to mch files")
parser.add_argument("-collection_name", help="Name of the SPMI collection to be done (e.g., libraries, tests)")
parser.add_argument("-collection_type", help="Type of the SPMI collection to be done (crossgen, crossgen2, pmi)")
parser.add_argument("-input_directory", help="directory containing assemblies for which superpmi collection to be done")
parser.add_argument("-max_size", help="Max size of each partition in MB")
is_windows = platform.system() == "Windows"

native_binaries_to_ignore = [
    "api-ms-win-core-console-l1-1-0.dll",
    "api-ms-win-core-datetime-l1-1-0.dll",
    "api-ms-win-core-debug-l1-1-0.dll",
    "api-ms-win-core-errorhandling-l1-1-0.dll",
    "api-ms-win-core-file-l1-1-0.dll",
    "api-ms-win-core-file-l1-2-0.dll",
    "api-ms-win-core-file-l2-1-0.dll",
    "api-ms-win-core-handle-l1-1-0.dll",
    "api-ms-win-core-heap-l1-1-0.dll",
    "api-ms-win-core-interlocked-l1-1-0.dll",
    "api-ms-win-core-libraryloader-l1-1-0.dll",
    "api-ms-win-core-localization-l1-2-0.dll",
    "api-ms-win-core-memory-l1-1-0.dll",
    "api-ms-win-core-namedpipe-l1-1-0.dll",
    "api-ms-win-core-processenvironment-l1-1-0.dll",
    "api-ms-win-core-processthreads-l1-1-0.dll",
    "api-ms-win-core-processthreads-l1-1-1.dll",
    "api-ms-win-core-profile-l1-1-0.dll",
    "api-ms-win-core-rtlsupport-l1-1-0.dll",
    "api-ms-win-core-string-l1-1-0.dll",
    "api-ms-win-core-synch-l1-1-0.dll",
    "api-ms-win-core-synch-l1-2-0.dll",
    "api-ms-win-core-sysinfo-l1-1-0.dll",
    "api-ms-win-core-timezone-l1-1-0.dll",
    "api-ms-win-core-util-l1-1-0.dll",
    "api-ms-win-crt-conio-l1-1-0.dll",
    "api-ms-win-crt-convert-l1-1-0.dll",
    "api-ms-win-crt-environment-l1-1-0.dll",
    "api-ms-win-crt-filesystem-l1-1-0.dll",
    "api-ms-win-crt-heap-l1-1-0.dll",
    "api-ms-win-crt-locale-l1-1-0.dll",
    "api-ms-win-crt-math-l1-1-0.dll",
    "api-ms-win-crt-multibyte-l1-1-0.dll",
    "api-ms-win-crt-private-l1-1-0.dll",
    "api-ms-win-crt-process-l1-1-0.dll",
    "api-ms-win-crt-runtime-l1-1-0.dll",
    "api-ms-win-crt-stdio-l1-1-0.dll",
    "api-ms-win-crt-string-l1-1-0.dll",
    "api-ms-win-crt-time-l1-1-0.dll",
    "api-ms-win-crt-utility-l1-1-0.dll",
    "clretwrc.dll",
    "clrgc.dll",
    "clrjit.dll",
    "clrjit_unix_arm_arm.dll",
    "clrjit_unix_arm_arm64.dll",
    "clrjit_unix_arm_x64.dll",
    "clrjit_unix_arm_x86.dll",
    "clrjit_unix_arm64_arm64.dll",
    "clrjit_unix_arm64_x64.dll",
    "clrjit_unix_armel_arm.dll",
    "clrjit_unix_armel_arm64.dll",
    "clrjit_unix_armel_x64.dll",
    "clrjit_unix_armel_x86.dll",
    "clrjit_unix_osx_arm64_arm64.dll",
    "clrjit_unix_osx_arm64_x64.dll",
    "clrjit_unix_x64_arm64.dll",
    "clrjit_unix_x64_x64.dll",
    "clrjit_win_arm_arm.dll",
    "clrjit_win_arm_arm64.dll",
    "clrjit_win_arm_x64.dll",
    "clrjit_win_arm_x86.dll",
    "clrjit_win_arm64_arm64.dll",
    "clrjit_win_arm64_x64.dll",
    "clrjit_win_x64_arm64.dll",
    "clrjit_win_x64_x64.dll",
    "clrjit_win_x86_arm.dll",
    "clrjit_win_x86_arm64.dll",
    "clrjit_win_x86_x64.dll",
    "clrjit_win_x86_x86.dll",
    "clrjit_universal_arm_arm.dll",
    "clrjit_universal_arm_arm64.dll",
    "clrjit_universal_arm_x64.dll",
    "clrjit_universal_arm_x86.dll",
    "clrjit_universal_arm64_arm64.dll",
    "clrjit_universal_arm64_x64.dll",
    "coreclr.dll",
    "CoreConsole.exe",
    "coredistools.dll",
    "CoreRun.exe",
    "CoreShim.dll",
    "createdump.exe",
    "crossgen.exe",
    "crossgen2.exe",
    "dbgshim.dll",
    "ilasm.exe",
    "ildasm.exe",
    "jitinterface_arm.dll",
    "jitinterface_arm64.dll",
    "jitinterface_x64.dll",
    "jitinterface_x86.dll",
    "KernelTraceControl.dll",
    "KernelTraceControl.Win61.dll",
    "mcs.exe",
    "Microsoft.DiaSymReader.Native.amd64.dll",
    "Microsoft.DiaSymReader.Native.x86.dll",
    "mscordaccore.dll",
    "mscordbi.dll",
    "mscorrc.dll",
    "msdia140.dll",
    "msquic.dll",
    "msvcp140.dll",
    "vcruntime140.dll",
    "vcruntime140_1.dll",
    "R2RDump.exe",
    "R2RTest.exe",
    "superpmi.exe",
    "superpmi-shim-collector.dll",
    "superpmi-shim-counter.dll",
    "superpmi-shim-simple.dll",
    "System.CommandLine.resources.dll", # Managed, but uninteresting
    "System.IO.Compression.Native.dll",
    "ucrtbase.dll",
    "xunit.console.exe",

    # The following cause VM asserts. Disabling to attempt to fix periodic SPMI collection pipeline failures.
    # Tracked by: https://github.com/dotnet/runtime/issues/70293
    "System.Runtime.InteropServices.Tests.dll", # libraries test
    "DllImportPathTest.dll", # coreclr test
]

MAX_FILES_COUNT = 1500


def setup_args(args):
    """ Setup the args for SuperPMI to use.

    Args:
        args (ArgParse): args parsed by arg parser

    Returns:
        args (CoreclrArguments)

    """
    coreclr_args = CoreclrArguments(args, require_built_core_root=False, require_built_product_dir=False,
                                    require_built_test_dir=False, default_build_type="Checked")

    coreclr_args.verify(args,
                        "source_directory",
                        lambda source_directory: os.path.isdir(source_directory),
                        "source_directory doesn't exist")

    coreclr_args.verify(args,
                        "core_root_directory",
                        lambda core_root_directory: os.path.isdir(core_root_directory),
                        "core_root_directory doesn't exist")

    coreclr_args.verify(args,
                        "arch",
                        lambda unused: True,
                        "Unable to set arch")

    coreclr_args.verify(args,
                        "platform",
                        lambda unused: True,
                        "Unable to set platform")

    coreclr_args.verify(args,
                        "mch_file_tag",
                        lambda unused: True,
                        "Unable to set mch_file_tag")

    coreclr_args.verify(args,
                        "collection_name",
                        lambda unused: True,
                        "Unable to set collection_name")

    coreclr_args.verify(args,
                        "collection_type",
                        lambda unused: True,
                        "Unable to set collection_type")

    coreclr_args.verify(args,
                        "input_directory",
                        lambda input_directory: os.path.isdir(input_directory),
                        "input_directory doesn't exist")

    coreclr_args.verify(args,
                        "max_size",
                        lambda max_size: max_size > 0,
                        "Please enter valid positive numeric max_size",
                        modify_arg=lambda max_size: int(
                            max_size) * 1000 * 1000 if max_size is not None and max_size.isnumeric() else 0
                        # Convert to MB
                        )
    return coreclr_args


def get_files_sorted_by_size(src_directory, exclude_directories, exclude_files):
    """ For a given src_directory, returns all the .dll files sorted by size.

    Args:
        src_directory (string): Path of directory to enumerate.
        exclude_directories ([string]): Directory names to exclude.
        exclude_files ([string]): File names to exclude.
    """

    def sorter_by_size(pair):
        """ Sorts the pair (file_name, file_size) tuple in descending order of file_size

        Args:
            pair ([(string, int)]): List of tuple of file_name, file_size
        """
        pair.sort(key=lambda x: x[1], reverse=True)
        return pair

    filename_with_size = []
    exclude_files_lower = [filename.lower() for filename in exclude_files]

    for file_path, dirs, files in os.walk(src_directory, topdown=True):
        # Credit: https://stackoverflow.com/a/19859907
        dirs[:] = [d for d in dirs if d not in exclude_directories]
        for name in files:
            # Make the exclude check case-insensitive
            if name.lower() in exclude_files_lower:
                continue
            curr_file_path = os.path.join(file_path, name)

            if not os.path.isfile(curr_file_path):
                continue
            if not name.endswith(".dll") and not name.endswith(".exe"):
                continue

            size = os.path.getsize(curr_file_path)
            filename_with_size.append((curr_file_path, size))

    return sorter_by_size(filename_with_size)


def first_fit(sorted_by_size, max_size):
    """ Given a list of file names along with size in descending order, divides the files
    in number of buckets such that each bucket doesn't exceed max_size (unless a single file exceeds
    max_size, in which case it gets its own bucket). Since this is a first-fit
    approach, it doesn't guarantee to find the bucket with tighest spot available.

    Args:
        sorted_by_size ((string, int)): (file_name, file_size) tuple
        max_size (int): Maximum size (in bytes) of each bucket.

    Returns:
        [{int, [string]}]: Returns a dictionary of partition-index to list of file names following in that bucket.
    """
    partitions = {}
    for curr_file in sorted_by_size:
        _, file_size = curr_file

        # Find the right bucket
        found_bucket = False

        if file_size < max_size:
            for p_index in partitions:
                total_in_curr_par = sum(n for _, n in partitions[p_index])
                if ((total_in_curr_par + file_size) < max_size) and (len(partitions[p_index]) < MAX_FILES_COUNT):
                    partitions[p_index].append(curr_file)
                    found_bucket = True
                    break

        if not found_bucket:
            partitions[len(partitions)] = [curr_file]

    total_size = 0
    for p_index in partitions:
        partition_size = sum(n for _, n in partitions[p_index])
        print("Partition {0}: {1} files with {2} bytes.".format(p_index, len(partitions[p_index]), partition_size))
        total_size += partition_size
    print("Total {0} partitions with {1} bytes.".format(str(len(partitions)), total_size))

    return partitions


def partition_files(src_directory, dst_directory, max_size, exclude_directories=[],
                    exclude_files=native_binaries_to_ignore):
    """ Copy bucketized files based on size to destination folder.

    Args:
        src_directory (string): Source folder containing files to be copied.
        dst_directory (string): Destination folder where files should be copied.
        max_size (int): Maximum partition size in bytes
        exclude_directories ([string]): List of folder names to be excluded.
        exclude_files ([string]): List of files names to be excluded.
    """

    print('Partitioning files from {0} to {1}'.format(src_directory, dst_directory))
    sorted_by_size = get_files_sorted_by_size(src_directory, exclude_directories, exclude_files)
    partitions = first_fit(sorted_by_size, max_size)

    index = 0
    for p_index in partitions:
        file_names = [curr_file[0] for curr_file in partitions[p_index]]
        curr_dst_path = os.path.join(dst_directory, str(index), "binaries")
        copy_files(src_directory, curr_dst_path, file_names)
        index += 1


def setup_microbenchmark(workitem_directory, arch):
    """ Perform setup of microbenchmarks

    Args:
        workitem_directory (string): Path to work
        arch (string): Architecture for which dotnet will be installed
    """
    performance_directory = os.path.join(workitem_directory, "performance")

    run_command(
        ["git", "clone", "--quiet", "--depth", "1", "https://github.com/dotnet/performance", performance_directory])

    with ChangeDir(performance_directory):
        dotnet_directory = os.path.join(performance_directory, "tools", "dotnet", arch)
        dotnet_install_script = os.path.join(performance_directory, "scripts", "dotnet.py")

        if not os.path.isfile(dotnet_install_script):
            print("Missing " + dotnet_install_script)
            return

        run_command(
            get_python_name() + [dotnet_install_script, "install", "--architecture", arch, "--install-dir",
                                 dotnet_directory, "--verbose"])


def get_python_name():
    """Gets the python name

    Returns:
        [string]: Returns the appropriate python name depending on the OS.
    """
    if is_windows:
        return ["py", "-3"]
    else:
        return ["python3"]


def main(main_args):
    """ Main entrypoint

    Args:
        main_args ([type]): Arguments to the script
    """
    coreclr_args = setup_args(main_args)
    source_directory = coreclr_args.source_directory

    # CorrelationPayload directories
    correlation_payload_directory = os.path.join(coreclr_args.source_directory, "payload")
    superpmi_src_directory = os.path.join(source_directory, 'src', 'coreclr', 'scripts')
    superpmi_dst_directory = os.path.join(correlation_payload_directory, "superpmi")
    arch = coreclr_args.arch
    platform_name = coreclr_args.platform.lower()
    helix_source_prefix = "official"
    creator = ""
    ci = True
    if platform_name == "windows":
        helix_queue = "Windows.10.Arm64" if arch == "arm64" else "Windows.10.Amd64.X86.Rt"
    elif platform_name == "linux":
        if arch == "arm":
            helix_queue = "(Ubuntu.1804.Arm32)Ubuntu.1804.Armarch@mcr.microsoft.com/dotnet-buildtools/prereqs:ubuntu-18.04-helix-arm32v7-bfcd90a-20200121150440"
        elif arch == "arm64":
            helix_queue = "(Ubuntu.1804.Arm64)Ubuntu.1804.ArmArch@mcr.microsoft.com/dotnet-buildtools/prereqs:ubuntu-18.04-helix-arm64v8-20210531091519-97d8652"
        else:
            helix_queue = "Ubuntu.1804.Amd64"
    elif platform_name == "osx":
        helix_queue = "OSX.1100.ARM64" if arch == "arm64" else "OSX.1014.Amd64"

    # create superpmi directory
    print('Copying {} -> {}'.format(superpmi_src_directory, superpmi_dst_directory))
    copy_directory(superpmi_src_directory, superpmi_dst_directory, verbose_output=True, match_func=lambda path: any(path.endswith(extension) for extension in [".py"]))

    if platform_name == "windows":
        acceptable_copy = lambda path: any(path.endswith(extension) for extension in [".py", ".dll", ".exe", ".json"])
    else:
        acceptable_extensions = [".py", ".dll", ".json"]
        acceptable_extensions.append(".so" if platform_name == "linux" else ".dylib")
        # Need to accept files without any extension, which is how executable file's names look.
        acceptable_copy = lambda path: (os.path.basename(path).find(".") == -1) or any(path.endswith(extension) for extension in acceptable_extensions)

    print('Copying {} -> {}'.format(coreclr_args.core_root_directory, superpmi_dst_directory))
    copy_directory(coreclr_args.core_root_directory, superpmi_dst_directory, verbose_output=True, match_func=acceptable_copy)

    # Copy all the test files to CORE_ROOT
    # The reason is there are lot of dependencies with *.Tests.dll and to ensure we do not get
    # Reflection errors, just copy everything to CORE_ROOT so for all individual partitions, the
    # references will be present in CORE_ROOT.
    if coreclr_args.collection_name == "libraries_tests":
        print('Copying {} -> {}'.format(coreclr_args.input_directory, superpmi_dst_directory))

        def make_readable(folder_name):
            """Make file executable by changing the permission

            Args:
                folder_name (string): folder to mark with 744
            """
            if is_windows:
                return

            print("Inside make_readable")
            run_command(["ls", "-l", folder_name])
            for file_path, dirs, files in os.walk(folder_name, topdown=True):
                for d in dirs:
                    os.chmod(os.path.join(file_path, d),
                    # read+write+execute for owner
                    (stat.S_IRUSR | stat.S_IWUSR | stat.S_IXUSR) |
                    # read for group
                    (stat.S_IRGRP) |
                    # read for other
                    (stat.S_IROTH))

                for f in files:
                    os.chmod(os.path.join(file_path, f),
                    # read+write+execute for owner
                    (stat.S_IRUSR | stat.S_IWUSR | stat.S_IXUSR) |
                    # read for group
                    (stat.S_IRGRP) |
                    # read for other
                    (stat.S_IROTH))
            run_command(["ls", "-l", folder_name])

        make_readable(coreclr_args.input_directory)
        copy_directory(coreclr_args.input_directory, superpmi_dst_directory, verbose_output=True, match_func=acceptable_copy)

    # Workitem directories
    workitem_directory = os.path.join(source_directory, "workitem")
    input_artifacts = ""

    if coreclr_args.collection_name == "benchmarks":
        # Setup microbenchmarks
        setup_microbenchmark(workitem_directory, arch)
    else:
        # Setup for pmi/crossgen runs

        # Clone and build jitutils
        try:
            with TempDir() as jitutils_directory:
                run_command(
                    ["git", "clone", "--quiet", "--depth", "1", "https://github.com/dotnet/jitutils", jitutils_directory])

                # Make sure ".dotnet" directory exists, by running the script at least once
                dotnet_script_name = "dotnet.cmd" if is_windows else "dotnet.sh"
                dotnet_script_path = os.path.join(source_directory, dotnet_script_name)
                run_command([dotnet_script_path, "--info"], jitutils_directory)

                # Set dotnet path to run build
                os.environ["PATH"] = os.path.join(source_directory, ".dotnet") + os.pathsep + os.environ["PATH"]
                build_file = "build.cmd" if is_windows else "build.sh"
                run_command([os.path.join(jitutils_directory, build_file), "-p"], jitutils_directory)

                copy_files(os.path.join(jitutils_directory, "bin"), superpmi_dst_directory, [os.path.join(jitutils_directory, "bin", "pmi.dll")])
        except PermissionError as pe_error:
            # Details: https://bugs.python.org/issue26660
            print('Ignoring PermissionError: {0}'.format(pe_error))

        # NOTE: we can't use the build machine ".dotnet" to run on all platforms. E.g., the Windows x86 build uses a
        # Windows x64 .dotnet\dotnet.exe that can't load a 32-bit shim. Thus, we always use corerun from Core_Root to invoke crossgen2.
        # The following will copy .dotnet to the correlation payload in case we change our mind, and need or want to use it for some scenarios.

        # # Copy ".dotnet" to correlation_payload_directory for crossgen2 job; it is needed to invoke crossgen2.dll
        # if coreclr_args.collection_type == "crossgen2":
        #     dotnet_src_directory = os.path.join(source_directory, ".dotnet")
        #     dotnet_dst_directory = os.path.join(correlation_payload_directory, ".dotnet")
        #     print('Copying {} -> {}'.format(dotnet_src_directory, dotnet_dst_directory))
        #     copy_directory(dotnet_src_directory, dotnet_dst_directory, verbose_output=False)

        # payload
        pmiassemblies_directory = os.path.join(workitem_directory, "pmiAssembliesDirectory")
        input_artifacts = os.path.join(pmiassemblies_directory, coreclr_args.collection_name)
        exclude_directory = ['Core_Root'] if coreclr_args.collection_name == "coreclr_tests" else []
        exclude_files = native_binaries_to_ignore
        if coreclr_args.collection_type == "crossgen2":
            print('Adding exclusions for crossgen2')
            # Currently, trying to crossgen2 R2RTest\Microsoft.Build.dll causes a pop-up failure, so exclude it.
            exclude_files += ["Microsoft.Build.dll"]

        if coreclr_args.collection_name == "libraries_tests":
            # libraries_tests artifacts contains files from core_root folder. Exclude them.
            core_root_dir = coreclr_args.core_root_directory
            exclude_files += [item for item in os.listdir(core_root_dir)
                              if os.path.isfile(os.path.join(core_root_dir, item)) and (item.endswith(".dll") or item.endswith(".exe"))]

        partition_files(coreclr_args.input_directory, input_artifacts, coreclr_args.max_size, exclude_directory,
                        exclude_files)

    # Set variables
    print('Setting pipeline variables:')
    set_pipeline_variable("CorrelationPayloadDirectory", correlation_payload_directory)
    set_pipeline_variable("WorkItemDirectory", workitem_directory)
    set_pipeline_variable("InputArtifacts", input_artifacts)
    set_pipeline_variable("Python", ' '.join(get_python_name()))
    set_pipeline_variable("Architecture", arch)
    set_pipeline_variable("Creator", creator)
    set_pipeline_variable("Queue", helix_queue)
    set_pipeline_variable("HelixSourcePrefix", helix_source_prefix)
    set_pipeline_variable("MchFileTag", coreclr_args.mch_file_tag)


################################################################################
# __main__
################################################################################

if __name__ == "__main__":
    args = parser.parse_args()
    sys.exit(main(args))
