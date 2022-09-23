#if UNITY_EDITOR || (MODIO_COMPILE_ALL && UNITY_EDITOR)

using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable 1998 // These async functions don't use await!

namespace ModIO.Implementation.Platform
{
    /// <summary>Editor implementation of the data services.</summary>
    internal class EditorDataService : IUserDataService, IPersistentDataService, ITempDataService
    {
        /// <summary>Root directory for all data services.</summary>
        public readonly static string GlobalRootDirectory =
            $@"{System.IO.Directory.GetCurrentDirectory()}/mod.io";

#region Data

        /// <summary>Root directory for the data service.</summary>
        string rootDir = null;

        /// <summary>Root directory for the data service.</summary>
        public string RootDirectory
        {
            get {
                return this.rootDir;
            }
        }

#endregion // Data

#region Initialization

        /// <summary>Init as IUserDataService.</summary>
        async Task<Result> IUserDataService.InitializeAsync(string userProfileIdentifier,
                                                            long gameId, BuildSettings settings)
        {
            // TODO(@jackson): Test dir creation
            this.rootDir =
                $"{EditorDataService.GlobalRootDirectory}/{gameId.ToString("00000")}/users/{userProfileIdentifier}";

            Logger.Log(LogLevel.Verbose, "Initialized EditorUserDataService: " + rootDir);

            return ResultBuilder.Success;
        }

        /// <summary>Init as IPersistentDataService.</summary>
        async Task<Result> IPersistentDataService.InitializeAsync(long gameId,
                                                                  BuildSettings settings)
        {
            // TODO(@jackson): Test dir creation
            this.rootDir =
                $"{EditorDataService.GlobalRootDirectory}/{gameId.ToString("00000")}/data";

            Logger.Log(LogLevel.Verbose, "Initialized EditorDataService as IPDS: " + rootDir);

            return ResultBuilder.Success;
        }

        /// <summary>Init as ITempDataService.</summary>
        async Task<Result> ITempDataService.InitializeAsync(long gameId, BuildSettings settings)
        {
            // TODO(@jackson): Test dir creation
            this.rootDir =
                $"{EditorDataService.GlobalRootDirectory}/{gameId.ToString("00000")}/temp";

            Logger.Log(LogLevel.Verbose, "Initialized EditorTempDataService: " + rootDir);

            return ResultBuilder.Success;
        }

#endregion // Initialization

#region Operations

        /// <summary>Opens a file stream for reading.</summary>
        public ModIOFileStream OpenReadStream(string filePath, out Result result)
        {
            DebugUtil.AssertPathValid(filePath, this.rootDir);
            return SystemIOWrapper.OpenReadStream(filePath, out result);
        }

        /// <summary>Opens a file stream for writing.</summary>
        public ModIOFileStream OpenWriteStream(string filePath, out Result result)
        {
            DebugUtil.AssertPathValid(filePath, this.rootDir);
            return SystemIOWrapper.OpenWriteStream(filePath, out result);
        }

        /// <summary>Reads an entire file asynchronously.</summary>
        public async Task<ResultAnd<byte[]>> ReadFileAsync(string filePath)
        {
            DebugUtil.AssertPathValid(filePath, this.rootDir);
            return await SystemIOWrapper.ReadFileAsync(filePath);
        }

        /// <summary>Writes an entire file asynchronously.</summary>
        public async Task<Result> WriteFileAsync(string filePath, byte[] data)
        {
            DebugUtil.AssertPathValid(filePath, this.rootDir);
            return await SystemIOWrapper.WriteFileAsync(filePath, data);
        }

        /// <summary>Deletes a file.</summary>
        public async Task<Result> DeleteFileAsync(string filePath)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Deletes a directory and its contents recursively.</summary>
        public Result DeleteDirectory(string directoryPath)
        {
            DebugUtil.AssertPathValid(directoryPath, this.rootDir);
            return SystemIOWrapper.DeleteDirectory(directoryPath);
        }

#endregion // Operations

#region Utility

        /// <summary>Determines whether a file exists.</summary>
        public bool FileExists(string filePath)
        {
            DebugUtil.AssertPathValid(filePath, this.rootDir);
            return SystemIOWrapper.FileExists(filePath, out Result r);
        }

        /// <summary>Lists all the files in the given directory recursively.</summary>
        public ResultAnd<List<string>> ListAllFiles(string directoryPath)
        {
            return SystemIOWrapper.ListAllFiles(directoryPath);
        }

        /// <summary>Gets the size and hash of a file.</summary>
        public async Task<ResultAnd<(long fileSize, string fileHash)>> GetFileSizeAndHash(
            string filePath)
        {
            DebugUtil.AssertPathValid(filePath, this.rootDir);
            return await SystemIOWrapper.GetFileSizeAndHash(filePath);
        }

        /// <summary>Determines whether a directory exists.</summary>
        public bool DirectoryExists(string directoryPath)
        {
            DebugUtil.AssertPathValid(directoryPath, this.rootDir);
            return SystemIOWrapper.DirectoryExists(directoryPath);
        }

        /// <summary>Determines whether a path can be handled by this data service.</summary>
        // public bool CanHandlePath(string path)
        // {
        //     // NOTE(@jackson): For EditorDataService, all services handle all paths
        //     return true;
        // }

#endregion // Utility
    }
}
#pragma warning restore 1998 // These async functions don't use await!

#endif // UNITY_EDITOR || (MODIO_COMPILE_ALL && UNITY_EDITOR)
