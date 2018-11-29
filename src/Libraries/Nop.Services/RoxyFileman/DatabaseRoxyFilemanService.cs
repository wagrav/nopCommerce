using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Services.Media;

namespace Nop.Services.RoxyFileman
{
    /// <summary>
    /// Database RoxyFileman service
    /// </summary>
    public class DatabaseRoxyFilemanService : FileRoxyFilemanService
    {
        #region Fields

        private readonly IPictureService _pictureService;

        #endregion
        
        #region Ctor

        public DatabaseRoxyFilemanService(IPictureService pictureService,
            IHostingEnvironment hostingEnvironment,
            IHttpContextAccessor httpContextAccessor,
            INopFileProvider fileProvider) : base(hostingEnvironment, httpContextAccessor, fileProvider)
        {
            this._pictureService = pictureService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get directories in the passed parent directory
        /// </summary>
        /// <param name="parentDirectoryPath">Path to the parent directory</param>
        /// <returns>Array of the paths to the directories</returns>
        protected override ArrayList GetDirectories(string parentDirectoryPath)
        {
            CreateDirectory(parentDirectoryPath);

            return base.GetDirectories(parentDirectoryPath);
        }

        /// <summary>
        /// Gets picture from database by file
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>Exist picture from database or null</returns>
        protected virtual Picture GetPictureByFile(string filePath)
        {
            var sourceVirtualPath = _fileProvider.GetVirtualPath(_fileProvider.GetDirectoryName(filePath));
            var fileName = _fileProvider.GetFileNameWithoutExtension(filePath);

            Picture picture = null;

            if (int.TryParse(fileName.Split('_')[0], out var pictureId))
            {
                picture = _pictureService.GetPictureById(pictureId);
            }

            return picture ?? _pictureService.GetPictures(sourceVirtualPath.TrimEnd('/'))
                       .FirstOrDefault(p => fileName.Contains(p.SeoFilename));
        }

        /// <summary>
        /// Create the passed directory
        /// </summary>
        /// <param name="directoryPath">Path to the parent directory</param>
        protected virtual void CreateDirectory(string directoryPath)
        {
            _fileProvider.CreateDirectory(directoryPath);
            var virtualPath = _fileProvider.GetVirtualPath(directoryPath).TrimEnd('/');
            var directoryNames = _pictureService.GetPictures($"{virtualPath}/")
                .Where(picture => picture.VirtualPath != virtualPath)
                .Select(picture => _fileProvider.GetAbsolutePath(picture.VirtualPath.TrimStart('~').Split('/')))
                .Distinct();

            foreach (var directory in directoryNames)
            {
                CreateDirectory(directory);
            }
        }

        /// <summary>
        /// Get files in the passed directory
        /// </summary>
        /// <param name="directoryPath">Path to the files directory</param>
        /// <param name="type">Type of the files</param>
        /// <returns>List of paths to the files</returns>
        protected override List<string> GetFiles(string directoryPath, string type)
        {
            if (type == "#")
                type = string.Empty;

            var files = new List<string>();

            //store files on disk if needed
            FlushImagesOnDisk(directoryPath);

            foreach (var fileName in _fileProvider.GetFiles(_fileProvider.DirectoryExists(directoryPath) ? directoryPath : GetFullPath(directoryPath)))
            {
                if (string.IsNullOrEmpty(type) || GetFileType(_fileProvider.GetFileExtension(fileName)) == type)
                    files.Add(fileName);
            }

            return files;
        }

        /// <summary>
        /// Сopy the directory with the embedded files and directories
        /// </summary>
        /// <param name="sourcePath">Path to the source directory</param>
        /// <param name="destinationPath">Path to the destination directory</param>
        protected override void CopyDirectory(string sourcePath, string destinationPath)
        {
            var pictures = _pictureService.GetPictures($"{_fileProvider.GetVirtualPath(sourcePath).TrimEnd('/')}/");
            var baseDestinationPathVirtualPath = _fileProvider.GetVirtualPath(destinationPath);

            foreach (var picture in pictures)
            {
                var destinationPathVirtualPath =
                    $"{baseDestinationPathVirtualPath.TrimEnd('/')}{picture.VirtualPath.Replace(_fileProvider.GetVirtualPath(sourcePath), "")}";

                _pictureService.InsertPicture(new RoxyFilemanFormFile(picture, _pictureService.GetFileExtensionFromMimeType(picture.MimeType)), string.Empty, destinationPathVirtualPath);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Flush images on disk
        /// </summary>
        /// <param name="directoryPath">Directory path to flush images</param>
        public override void FlushImagesOnDisk(string directoryPath)
        {
            base.FlushImagesOnDisk(directoryPath);

            foreach (var picture in _pictureService.GetPictures(_fileProvider.GetVirtualPath(directoryPath)))
            {
                int.TryParse(GetSetting("MAX_IMAGE_WIDTH"), out var width);
                int.TryParse(GetSetting("MAX_IMAGE_HEIGHT"), out var height);

                //save picture to folder if its not exists
                _pictureService.GetPictureUrl(picture, width > height ? width : height);
            }
        }

        /// <summary>
        /// Copy the file
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="destinationPath">Path to the destination file</param>
        /// <returns>A task that represents the completion of the operation</returns>
        public override async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            var filePath = _fileProvider.GetAbsolutePath(_fileProvider.GetVirtualPath(sourcePath));
            var picture = GetPictureByFile(filePath);

            if (picture == null)
                throw new Exception(GetLanguageResource("E_CopyFile"));

            _pictureService.InsertPicture(
                new RoxyFilemanFormFile(picture, _fileProvider.GetFileExtension(filePath)),
                string.Empty, _fileProvider.GetVirtualPath(destinationPath));

            await HttpContext.Response.WriteAsync(GetSuccessResponse());
        }

        /// <summary>
        /// Delete the file
        /// </summary>
        /// <param name="sourcePath">Path to the file</param>
        /// <returns>A task that represents the completion of the operation</returns>
        public override async Task DeleteFileAsync(string sourcePath)
        {
            var filePath = _fileProvider.GetAbsolutePath(_fileProvider.GetVirtualPath(sourcePath));
            var picture = GetPictureByFile(filePath);

            if (picture == null)
                throw new Exception(GetLanguageResource("E_CopyFile"));

            _pictureService.DeletePicture(picture);

            await base.DeleteFileAsync(sourcePath);
        }

        /// <summary>
        /// Move the directory
        /// </summary>
        /// <param name="sourcePath">Path to the source directory</param>
        /// <param name="destinationPath">Path to the destination directory</param>
        /// <returns>A task that represents the completion of the operation</returns>
        public override async Task MoveDirectoryAsync(string sourcePath, string destinationPath)
        {
            await base.MoveDirectoryAsync(sourcePath, destinationPath);

            var pictures = _pictureService.GetPictures($"{_fileProvider.GetVirtualPath(sourcePath).TrimEnd('/')}/");
            var baseDestinationPathVirtualPath = _fileProvider.GetVirtualPath(destinationPath);

            foreach (var picture in pictures)
            {
                var destinationPathVirtualPath =
                    $"{baseDestinationPathVirtualPath.TrimEnd('/')}/{_fileProvider.GetDirectoryNameOnly(_fileProvider.GetAbsolutePath(sourcePath.TrimStart('~').Split('/')))}";

                picture.VirtualPath = destinationPathVirtualPath;

                _pictureService.UpdatePicture(picture);
            }
        }

        /// <summary>
        /// Move the file
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="destinationPath">Path to the destination file</param>
        /// <returns>A task that represents the completion of the operation</returns>
        public override async Task MoveFileAsync(string sourcePath, string destinationPath)
        {
            await base.MoveFileAsync(sourcePath, destinationPath);

            var filePath = _fileProvider.GetAbsolutePath(_fileProvider.GetVirtualPath(sourcePath));
            var picture = GetPictureByFile(filePath);

            if (picture == null)
                throw new Exception(GetLanguageResource("E_CopyFile"));

            picture.VirtualPath = _fileProvider.GetVirtualPath(_fileProvider.GetVirtualPath(_fileProvider.GetDirectoryName(destinationPath)));
            _pictureService.UpdatePicture(picture);
        }

        /// <summary>
        /// Rename the directory
        /// </summary>
        /// <param name="sourcePath">Path to the source directory</param>
        /// <param name="newName">New name of the directory</param>
        /// <returns>A task that represents the completion of the operation</returns>
        public override async Task RenameDirectoryAsync(string sourcePath, string newName)
        {
            var sourceVirtualPath = _fileProvider.GetVirtualPath(sourcePath).TrimEnd('/');
            var pictures = _pictureService.GetPictures($"{sourceVirtualPath}/");

            var destinationPath =
                $"{_fileProvider.GetVirtualPath(_fileProvider.GetParentDirectory(_fileProvider.GetAbsolutePath(sourcePath.Split('/')))).TrimEnd('/')}/{newName}";
            
            foreach (var picture in pictures)
            {
                picture.VirtualPath = destinationPath;

                _pictureService.UpdatePicture(picture);
            }

            await base.RenameDirectoryAsync(sourcePath, newName);
        }

        /// <summary>
        /// Rename the file
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="newName">New name of the file</param>
        /// <returns>A task that represents the completion of the operation</returns>
        public override async Task RenameFileAsync(string sourcePath, string newName)
        {
            var filePath = _fileProvider.GetAbsolutePath(_fileProvider.GetVirtualPath(sourcePath));
            var picture = GetPictureByFile(filePath);

            if (picture == null)
                throw new Exception(GetLanguageResource("E_CopyFile"));

            picture.SeoFilename = _fileProvider.GetFileNameWithoutExtension(newName);

            _pictureService.UpdatePicture(picture);

            await base.DeleteFileAsync(sourcePath);
        }

        /// <summary>
        /// Upload files to a directory on passed path
        /// </summary>
        /// <param name="directoryPath">Path to directory to upload files</param>
        /// <returns>A task that represents the completion of the operation</returns>
        public override async Task UploadFilesAsync(string directoryPath)
        {
            var result = GetSuccessResponse();
            var hasErrors = false;
            try
            {
                var fullPath = GetFullPath(GetVirtualPath(directoryPath));
                foreach (var formFile in HttpContext.Request.Form.Files)
                {
                    var fileName = formFile.FileName;
                    if (CanHandleFile(fileName))
                    {
                        var uniqueFileName = GetUniqueFileName(fullPath, _fileProvider.GetFileName(fileName));
                        var destinationFile = _fileProvider.Combine(fullPath, uniqueFileName);

                        if (GetFileType(new FileInfo(uniqueFileName).Extension) != "image")
                        {
                            using (var stream = new FileStream(destinationFile, FileMode.OpenOrCreate))
                            {
                                formFile.CopyTo(stream);
                            }
                        }
                        else
                        {
                            _pictureService.InsertPicture(formFile, virtualPath: GetVirtualPath(directoryPath));
                        }
                    }
                    else
                    {
                        hasErrors = true;
                        result = GetErrorResponse(GetLanguageResource("E_UploadNotAll"));
                    }
                }
            }
            catch (Exception ex)
            {
                result = GetErrorResponse(ex.Message);
            }

            if (IsAjaxRequest())
            {
                if (hasErrors)
                    result = GetErrorResponse(GetLanguageResource("E_UploadNotAll"));

                await HttpContext.Response.WriteAsync(result);
            }
            else
                await HttpContext.Response.WriteAsync($"<script>parent.fileUploaded({result});</script>");
        }

        #endregion
    }
}