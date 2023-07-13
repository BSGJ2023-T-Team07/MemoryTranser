using System;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Title.UI {
    public class TitleSettingsShower : MonoBehaviour {
        [SerializeField] private GameObject settingsLayer;

        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Transform bgmSliderTextTransform;

        [SerializeField] private Slider seVolumeSlider;
        [SerializeField] private Transform seSliderTextTransform;

        [Space] [SerializeField] private float selectedScaleMultiplier;

        private Vector3 _bgmSliderDefaultScale;
        private Vector3 _bgmSliderTextDefaultScale;
        private Vector3 _seSliderDefaultScale;
        private Vector3 _seSliderTextDefaultScale;

        private void Awake() {
            settingsLayer.SetActive(false);
            _bgmSliderDefaultScale = bgmVolumeSlider.transform.localScale;
            _bgmSliderTextDefaultScale = bgmSliderTextTransform.localScale;
            _seSliderDefaultScale = seVolumeSlider.transform.localScale;
            _seSliderTextDefaultScale = seSliderTextTransform.localScale;
        }

        public void OpenSettings() {
            settingsLayer.SetActive(true);
            UpdateSettingsShow(SettingsSelection.BgmVolume);
        }

        public void CloseSettings() {
            settingsLayer.SetActive(false);
        }

        public void UpdateSettingsShow(SettingsSelection selection) {
            switch (selection) {
                case SettingsSelection.SeVolume:
                    ShowSeSelected();
                    ShowBgmNotSelected();
                    break;
                case SettingsSelection.BgmVolume:
                    ShowSeNotSelected();
                    ShowBgmSelected();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selection), selection, null);
            }
        }

        public void AddSeSliderValue(float value) {
            seVolumeSlider.value += value;
        }

        public void AddBgmSliderValue(float value) {
            bgmVolumeSlider.value += value;
        }

        private void ShowSeSelected() {
            seVolumeSlider.transform.localScale = _seSliderDefaultScale * selectedScaleMultiplier;
            seSliderTextTransform.localScale = _seSliderTextDefaultScale * selectedScaleMultiplier;
        }

        private void ShowSeNotSelected() {
            seVolumeSlider.transform.localScale = _seSliderDefaultScale;
            seSliderTextTransform.localScale = _seSliderTextDefaultScale;
        }

        private void ShowBgmSelected() {
            bgmVolumeSlider.transform.localScale = _bgmSliderDefaultScale * selectedScaleMultiplier;
            bgmSliderTextTransform.localScale = _bgmSliderTextDefaultScale * selectedScaleMultiplier;
        }

        private void ShowBgmNotSelected() {
            bgmVolumeSlider.transform.localScale = _bgmSliderDefaultScale;
            bgmSliderTextTransform.localScale = _bgmSliderTextDefaultScale;
        }
    }
}