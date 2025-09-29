using ESignature.Core.Helpers;
using ESignature.Hash.ServiceLayer.Settings;
using ESignature.ServiceLayer.Services.Dtos;
using ESignature.ServiceLayer.Settings;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ESignature.ServiceLayer.Services.OnStartup
{
    public class ApiSourceData
    {
        public IList<ApiSourceItemDto> Sources { get; private set; }

        //public List<RsspCloudSetting> Signers { get; private set; }
        public List<HashRsspCloudSetting> HashSigners { get; private set; }

        // danh sách fullname branch kí để kiểm soát việc xuống dòng của tên khi kí
        public List<Branch> Branches { get; private set; }

        public ApiSourceItemDto GetApiSource(string tokenKey)
        {
            return Sources.First(q => q.Key == tokenKey);
        }
        public HashRsspCloudSetting GetHashSigner(string signerId)
        {
            return HashSigners.FirstOrDefault(q => q.SignerId == signerId);
        }

        //public RsspCloudSetting GetSigner(string signerId)
        //{
        //    return Signers.FirstOrDefault(q => q.SignerId == signerId);
        //}

        // danh sách fullname branch kí để kiểm soát việc xuống dòng của tên khi kí
        public Branch GetBranch(string signerId)
        {
            return Branches.FirstOrDefault(q => q.SignerId == signerId);
        }
        public void SetSources(IList<ApiSourceItemDto> items)
        {
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.Folder))
                {
                    ServiceExtensions.EnsureFolder(Path.Combine(item.Folder, "pending"));
                    ServiceExtensions.EnsureFolder(Path.Combine(item.Folder, "completed"));
                }
            }
            Sources = items;
        }

        //public void SetSigners(List<RsspCloudSetting> items)
        //{
        //    Signers = items;
        //}
        public void SetHashSigners(List<HashRsspCloudSetting> items)
        {
            HashSigners = items;
        }
        // danh sách fullname branch kí để kiểm soát việc xuống dòng của tên khi kí
        public void SetBranchesSetting(List<Branch> items)
        {
            Branches = items;
        }
    }
}