using System;
using MemoryTranser.Scripts.Game.BrainEvent;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class BrainEventTypeShower : MonoBehaviour, IOnStateChangedToInitializing {
        [SerializeField] private PostProcessVolume postProcessVolume;

        [Space] [SerializeField] private PostProcessProfile nonePostProcessProfile;
        [SerializeField] private PostProcessProfile blindPostProcessProfile;
        [SerializeField] private PostProcessProfile desireOutbreakPostProcessProfile;
        [SerializeField] private PostProcessProfile invertControlPostProcessProfile;
        [SerializeField] private PostProcessProfile achievementOfStudyPostProcessProfile;
        [SerializeField] private PostProcessProfile feverTimePostProcessProfile;


        #region interfaceの実装

        public void OnStateChangedToInitializing() {
            SetPostProcessForNone();
        }

        #endregion

        public void SetBrainEventShow(BrainEventType brainEventType, float durationSec) {
            SetPostProcess(brainEventType);

            if (brainEventType != BrainEventType.None) {
                SetBrainEventAnnounce(brainEventType, durationSec);
            }
        }

        private static void SetBrainEventAnnounce(BrainEventType brainEventType, float durationSec) {
            AnnounceShower.I.AddAnnounceText(GetBrainEventText(brainEventType), durationSec);
        }

        private static string GetBrainEventText(BrainEventType brainEventType) {
            return brainEventType switch {
                BrainEventType.Blind => $"{brainEventType.ToJapanese()}発生中！",
                BrainEventType.DesireOutbreak => $"{brainEventType.ToJapanese()}発生中！",
                BrainEventType.InvertControl => $"{brainEventType.ToJapanese()}発生中！",
                BrainEventType.AchievementOfStudy => $"{brainEventType.ToJapanese()}が出た！",
                BrainEventType.FeverTime => $"{brainEventType.ToJapanese()}発生中！",
                _ => throw new ArgumentOutOfRangeException(nameof(brainEventType), brainEventType, null)
            };
        }

        private void SetPostProcess(BrainEventType brainEventType) {
            switch (brainEventType) {
                case BrainEventType.None:
                    SetPostProcessForNone();
                    break;
                case BrainEventType.Blind:
                    SetPostProcessForBlind();
                    break;
                case BrainEventType.DesireOutbreak:
                    SetPostProcessForDesireOutBreak();
                    break;
                case BrainEventType.InvertControl:
                    SetPostProcessForInvertControl();
                    break;
                case BrainEventType.AchievementOfStudy:
                    SetPostProcessForAchievementOfStudy();
                    break;
                case BrainEventType.FeverTime:
                    SetPostProcessForFeverTime();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(brainEventType), brainEventType, null);
            }
        }

        private void SetPostProcessForNone() {
            postProcessVolume.profile = nonePostProcessProfile;
        }

        private void SetPostProcessForDesireOutBreak() {
            postProcessVolume.profile = desireOutbreakPostProcessProfile;
        }

        private void SetPostProcessForBlind() {
            postProcessVolume.profile = blindPostProcessProfile;
        }

        private void SetPostProcessForAchievementOfStudy() {
            postProcessVolume.profile = achievementOfStudyPostProcessProfile;
        }

        private void SetPostProcessForInvertControl() {
            postProcessVolume.profile = invertControlPostProcessProfile;
        }

        private void SetPostProcessForFeverTime() {
            postProcessVolume.profile = feverTimePostProcessProfile;
        }
    }
}