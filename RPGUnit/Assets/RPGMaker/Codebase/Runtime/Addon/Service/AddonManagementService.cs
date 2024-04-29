using System;
using System.Collections.Generic;
using RPGMaker.Codebase.CoreSystem.Lib.Auth;

namespace RPGMaker.Codebase.Runtime.Addon
{
    public class AddonManagementService
    {
        private readonly AddonRepository _addonRepository;

        public AddonManagementService() {
            _addonRepository = new AddonRepository();
        }

        public List<AddonDataModel> LoadAddons() {
            return _addonRepository.GetAddonDataModels();
        }

        public void SaveAddons(List<AddonDataModel> addonDataModels) {
            _addonRepository.StoreAddonDataModels(addonDataModels);
        }

        public AddonInfoContainer LoadAddonInfos() {
            return _addonRepository.GetAddonInfos();
        }

        public void SaveAddonInfos(AddonInfoContainer addonInfos) {
            _addonRepository.StoreAddonInfos(addonInfos);
        }
    }
}
