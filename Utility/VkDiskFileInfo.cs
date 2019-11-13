

namespace VkDiskCore.Utility
{
	public struct VkDiskFileInfo
	{
		public string Src;
		public string Name;
		public string Folder;
		public string Extension;
		public long TotalSize; // bytes
		public long DoneSize; // bytes
		/// <summary>
		/// is download or upload <seealso cref="ConnectionInfo"/>
		/// </summary>
		public bool IsDownload;
		public long ByteSpeed; //speed of downloading / uploading
	}

}
