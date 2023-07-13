using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MemoryTranser.Scripts.Game.BrainEvent;
using MemoryTranser.Scripts.Game.Desire;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Phase;
using MemoryTranser.Scripts.Game.UI.Debug;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;
using Random = UnityEngine.Random;
using UniRx;

namespace MemoryTranser.Scripts.Game.MemoryBox {
    public class MemoryBoxManager : MonoBehaviour, IOnGameAwake, IOnStateChangedToFinished {
        #region ゲームオブジェクトの定義

        [SerializeField] private GameObject memoryBoxPrefab;
        [SerializeField] private GameObject spawnArea;
        [SerializeField] private BoxTypeProbabilityShower boxTypeProbabilityShower;
        [SerializeField] private DesireManager desireManager;
        [SerializeField] private PhaseManager phaseManager;
        [SerializeField] private BrainEventManager brainEventManager;

        #endregion

        #region 変数の定義

        [Header("MemoryBoxの最大生成数")] [SerializeField]
        private int maxBoxGenerateCount;

        [Header("丸いMemoryBoxの最大生成数")] [SerializeField]
        private int maxSphereBoxGenerateCount;

        [Header("勉強の成果イベントで考慮するフェイズの数")] [SerializeField]
        private int nearPhasesCount;

        [Header("勉強の成果イベントで消えるMemoryBoxの最大数")] [SerializeField]
        private int maxDisappearBoxCountOnStudy;

        [Header("MemoryBoxの重さの最大値")] [SerializeField]
        private float maxWeight;

        [Header("MemoryBoxの重さの最小値")] [SerializeField]
        private float minWeight;

        [Header("MemoryBoxが消えてから再生成されるまでの時間(秒)")] [SerializeField]
        private float generateIntervalSec;

        [Header("SphereBoxの質量の倍率")] [SerializeField]
        private float sphereBoxMassMagnification;

        [Header("SphereBoxのScaleの倍率")] [SerializeField]
        private float sphereBoxScaleMagnification;

        private MemoryBoxCore[] _allBoxes;
        private Queue<MemoryBoxCore> _appliedDisappearedBoxes;
        private bool[] _outputable;

        private float[] _initialBoxTypeProbWeights;
        private float[] _currentBoxTypeProbWeights;
        private AliasMethod _memoryTypeAliasMethod;
        private readonly AliasMethod _shapeTypeAliasMethod = new();

        #endregion

        #region Unityから呼ばれる

        private void Awake() {
            spawnArea.GetComponent<SpriteRenderer>().enabled = false;

            _outputable = new bool[maxBoxGenerateCount];
            _appliedDisappearedBoxes = new Queue<MemoryBoxCore>();

            _shapeTypeAliasMethod.Constructor(new[] { 3f, 1f });
        }

        #endregion

        private static void MakeBoxAppear(MemoryBoxCore box) {
            if (GameFlowManager.I.CurrentGameState is GameState.Result or GameState.Finished) {
                return;
            }

            box.isOutput = false;
            box.Trail.enabled = false;

            var nowColor = box.SpRr.color;
            box.SpRr.color = new Color(nowColor.r, nowColor.g, nowColor.b, 0);
            box.SpRr.enabled = true;
            box.SpRr.DOFade(1f, 0.5f);

            var myTransform = box.transform;
            var nowScale = myTransform.localScale;
            myTransform.localScale = Vector3.zero;
            myTransform.DOScale(nowScale, 0.5f);

            box.Cc2D.isTrigger = false;
            box.Cc2D.enabled = true;

            box.MyState = MemoryBoxState.PlacedOnLevel;
        }


        private MemoryBoxCore ApplyRandomParameterForMemoryBox(MemoryBoxCore memoryBoxCore) {
            //ランダムにパラメーターを決める
            var randomBoxType = GetRandomBoxType();
            var randomWeight = Random.Range(minWeight, maxWeight);

            var currentSphereBoxCount = _allBoxes.Count(box => {
                if (box) {
                    return box.BoxShapeType == MemoryBoxShapeType.Sphere;
                }
                else {
                    return false;
                }
            });

            MemoryBoxShapeType randomBoxShape;
            if (currentSphereBoxCount >= maxSphereBoxGenerateCount) {
                randomBoxShape = MemoryBoxShapeType.Cube;
            }
            else {
                randomBoxShape = (MemoryBoxShapeType)_shapeTypeAliasMethod.Roll();
            }


            //決定したパラメーターを代入する
            memoryBoxCore.BoxMemoryType = randomBoxType;
            memoryBoxCore.BoxShapeType = randomBoxShape;
            memoryBoxCore.Weight = randomWeight;
            if (memoryBoxCore.BoxShapeType == MemoryBoxShapeType.Cube) {
                memoryBoxCore.Rb2D.mass = randomWeight;
            }
            else {
                memoryBoxCore.Rb2D.mass = randomWeight * sphereBoxMassMagnification;
            }

            //決定したパラメーターから他の値に反映させる
            memoryBoxCore.SpRr.sprite = randomBoxType.ToMemoryBoxSprite(randomBoxShape);
            memoryBoxCore.Trail.widthMultiplier = randomWeight;
            memoryBoxCore.Trail.time = randomWeight;
            if (memoryBoxCore.BoxShapeType == MemoryBoxShapeType.Cube) {
                memoryBoxCore.transform.localScale = Vector3.one * memoryBoxCore.Weight / 1.2f;
            }
            else {
                memoryBoxCore.transform.localScale =
                    Vector3.one * (memoryBoxCore.Weight / 1.2f * sphereBoxScaleMagnification);
            }

            memoryBoxCore.gameObject.layer = LayerMask.NameToLayer($"{randomBoxShape.ToString()}MemoryBox");

            return memoryBoxCore;
        }


        private Vector3 GetRandomSpawnPosition() {
            var spawnAreaOrigin = (Vector2)spawnArea.transform.position;
            var spawnAreaSize = (Vector2)spawnArea.transform.localScale;

            var randomX = Random.Range(spawnAreaOrigin.x - spawnAreaSize.x / 2,
                spawnAreaOrigin.x + spawnAreaSize.x / 2);
            var randomY = Random.Range(spawnAreaOrigin.y - spawnAreaSize.y / 2,
                spawnAreaOrigin.y + spawnAreaSize.y / 2);
            return new Vector3(randomX, randomY, 0);
        }

        public void GenerateMemoryBoxes() {
            _allBoxes = new MemoryBoxCore[maxBoxGenerateCount];

            for (var i = 0; i < maxBoxGenerateCount; i++) {
                //MemoryBoxの生成
                var obj = Instantiate(memoryBoxPrefab, GetRandomSpawnPosition(), Quaternion.identity);
                var memoryBoxCore = obj.GetComponent<MemoryBoxCore>();

                //MemoryBoxにランダムなパラメーターを設定する
                ApplyRandomParameterForMemoryBox(memoryBoxCore);
                MakeBoxAppear(memoryBoxCore);

                //MemoryBoxがDisappearするときのイベントに登録する
                memoryBoxCore.OnDisappear.Subscribe(async _ => {
                    //MemoryBoxが消えてからは一定時間処理を待機
                    await UniTask.Delay(TimeSpan.FromSeconds(generateIntervalSec));

                    //待機後、MemoryBoxをAppearさせる準備を完了する
                    ApplyRandomParameterForMemoryBox(memoryBoxCore);
                    memoryBoxCore.transform.position = GetRandomSpawnPosition();

                    //もしDesireが存在しないなら、MemoryBoxをAppearさせる
                    if (desireManager.ExistingDesireCores.Count == 0) {
                        MakeBoxAppear(memoryBoxCore);
                        if (brainEventManager.OnBrainEventTransition.Value == BrainEventType.Blind) {
                            memoryBoxCore.SmokeParticle.Play();
                        }
                    }
                    else {
                        _appliedDisappearedBoxes.Enqueue(memoryBoxCore);
                    }
                });

                //IDの生成
                //_allBoxesのインデックスがIDとなる
                memoryBoxCore.BoxId = i;

                //MemoryBoxの監視
                _allBoxes[i] = memoryBoxCore;
            }

            //BrainEventが遷移したら煙のパーティクルを止める、というイベントを購読する
            brainEventManager.OnBrainEventTransition.Subscribe(_ => {
                foreach (var box in _allBoxes) {
                    box.SmokeParticle.Stop();
                }
            });

            //遷移したBrainEventがド忘れなら煙のパーティクルを再生する、というイベントを購読する
            brainEventManager.OnBrainEventTransition.Where(x => x == BrainEventType.Blind).Subscribe(_ => {
                foreach (var box in _allBoxes) {
                    if (box.MyState != MemoryBoxState.Disappeared) {
                        box.SmokeParticle.Play();
                    }
                }
            });

            //遷移したBrainEventが頭がスッキリだったらMemoryBoxを一部消す、というイベントを購読する
            brainEventManager.OnBrainEventTransition.Where(x => x == BrainEventType.AchievementOfStudy).Subscribe(_ => {
                var nearPhases = phaseManager.GetNearPhases(nearPhasesCount);
                var nearMemoryTypes = nearPhases.Select(x => x.QuestType).ToArray();
                var currentDisappearCount = 0;

                foreach (var box in _allBoxes) {
                    if (box.MyState == MemoryBoxState.Disappeared) {
                        continue;
                    }

                    if (!nearMemoryTypes.Contains(box.BoxMemoryType)) {
                        ApplyRandomParameterForMemoryBox(box);
                        currentDisappearCount++;
                    }

                    if (currentDisappearCount >= maxDisappearBoxCountOnStudy) {
                        break;
                    }
                }
            });
        }

        private void InitializeBoxTypeProbWeights() {
            _memoryTypeAliasMethod = new AliasMethod();
            _initialBoxTypeProbWeights = new float[(int)BoxMemoryType.Count];
            Array.Fill(_initialBoxTypeProbWeights, 1f);
            _currentBoxTypeProbWeights = _initialBoxTypeProbWeights;
        }

        private BoxMemoryType GetRandomBoxType() {
            var randomBoxType = (BoxMemoryType)_memoryTypeAliasMethod.Roll();
            return randomBoxType;
        }

        public void SetBoxTypeProbWeights(float[] boxTypeWeights) {
            _currentBoxTypeProbWeights = boxTypeWeights;
            _memoryTypeAliasMethod.Constructor(_currentBoxTypeProbWeights);
            boxTypeProbabilityShower.SetBoxTypeProbabilityText(_currentBoxTypeProbWeights);
        }

        public void AddOutputableId(int boxId) {
            _outputable[boxId] = true;
        }

        public void RemoveOutputableId(int boxId) {
            _outputable[boxId] = false;
        }


        public MemoryBoxCore[] GetOutputableBoxes() {
            var outputableBoxes = new List<MemoryBoxCore>();
            for (var i = 0; i < maxBoxGenerateCount; i++) {
                if (_outputable[i]) {
                    outputableBoxes.Add(_allBoxes[i]);
                }
            }

            return outputableBoxes.ToArray();
        }

        #region interfaceの実装

        public void OnGameAwake() {
            InitializeBoxTypeProbWeights();

            //ステージ上のDesireの数が0になったら消えていたMemoryBoxをAppearさせる、というeventを購読する
            desireManager.ExistingDesireCores.ObserveCountChanged().Where(x => x == 0).Subscribe(_ => {
                for (var i = 0; i < _appliedDisappearedBoxes.Count; i++) {
                    var box = _appliedDisappearedBoxes.Dequeue();
                    MakeBoxAppear(box);
                    if (brainEventManager.OnBrainEventTransition.Value == BrainEventType.Blind) {
                        box.SmokeParticle.Play();
                    }
                }
            });
        }

        public void OnStateChangedToFinished() { }

        #endregion
    }
}