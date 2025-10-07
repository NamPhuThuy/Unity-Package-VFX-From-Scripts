using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.VFX
{
    public class VFXResourceFly : VFXBase
    {
        #region EXAMPLE
        
        /* HOW TO SET-UP
         
         */
        
        /*
        // Using VFXManager
        VFXManager.Ins.PlayAt(
            type: VFXType.RESOURCE_FLY,
            pos: spawnPosition,
            amount: 250,
            target: coinUITransform,
            onArrive: () => Debug.Log("Resource arrived!"),
            onComplete: () => Debug.Log("Animation complete!")
        );

        // Or directly
        var resourceFly = GetComponent<VFXResourceFly>();
        var config = new ResourceFlyConfig
        {
            sprite = coinSprite,
            targetPos = coinUIPosition,
            targetTransform = coinUITransform,
            startValue = 100,
            endValue = 350
        };
        resourceFly.SetConfig(config);
        resourceFly.Play(new VFXArguments { amount = 250 });
        */
        #endregion

        #region Private Serializable Fields
        [Header("Resource Settings")]
        [SerializeField] private ResourceFlyConfig defaultConfig;
        
        [Header("Pool Settings")]
        [SerializeField] private int poolSize = 10;
        [SerializeField] private GameObject resourcePrefab;
        [SerializeField] private Transform container;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI fakeResourceText;
        [SerializeField] private TextMeshProUGUI realResourceText;

        [Header("Animation Settings")]
        [SerializeField] private float spawnDelay = 0.12f;
        [SerializeField] private float bounceHeight = 150f;
        [SerializeField] private float flyDuration = 0.4f;
        [SerializeField] private float scaleUpDuration = 0.3f;
        [SerializeField] private float scaleDownDuration = 0.2f;
        [SerializeField] private AnimationCurve flightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Effects")]
        [SerializeField] private ParticleSystem rippleEffect;
        [SerializeField] private float punchScaleAmount = 0.15f;
        [SerializeField] private float punchDuration = 0.3f;
        #endregion

        #region Private Fields
        private RectTransform[] _resourceItems;
        private Image[] _resourceImages;
        private ResourceFlyConfig _currentConfig;
        private Tweener _shakeTargetTween;
        private int _remainingItems;
        private float _unitValue;
        private int _currentDisplayValue;
        #endregion

        #region Events
        public System.Action OnResourceAnimationComplete;
        public System.Action<int> OnValueUpdated;
        #endregion

        #region MonoBehaviour Callbacks
        private void Awake()
        {
            CreatePool();
            _currentConfig = defaultConfig ?? new ResourceFlyConfig();
        }
        #endregion

        #region VFXBase Implementation
        protected override void OnPlay()
        {
            // Setup config from VFX arguments
            SetupConfigFromArgs();
            
            // Start the resource fly animation
            StartCoroutine(TriggerResourceFlyAnimation());
        }
        
        private void SetupConfigFromArgs()
        {
            // Use target transform if provided
            if (args.targetTransform != null)
            {
                _currentConfig.targetTransform = args.targetTransform;
                _currentConfig.targetPos = args.targetTransform.position;
            }

            // Set amount if provided
            if (args.amount > 0)
            {
                _currentConfig.endValue = _currentConfig.startValue + args.amount;
            }

            // Set duration if provided
            if (args.duration > 0)
            {
                flyDuration = args.duration;
            }
        }
        #endregion

        #region Private Methods
        private void CreatePool()
        {
            if (resourcePrefab == null || container == null)
            {
                Debug.LogError("Resource prefab or container is not assigned");
                return;
            }

            _resourceItems = new RectTransform[poolSize];
            _resourceImages = new Image[poolSize];

            for (int i = 0; i < poolSize; i++)
            {
                GameObject item = Instantiate(resourcePrefab, container);
                _resourceItems[i] = item.GetComponent<RectTransform>();
                _resourceImages[i] = item.GetComponent<Image>();

                if (_resourceImages[i] == null)
                {
                    Debug.LogError($"Resource prefab must have an Image component");
                }

                item.SetActive(false);
            }
        }

        private IEnumerator TriggerResourceFlyAnimation()
        {
            if (_currentConfig.sprite == null)
            {
                Debug.LogError("Resource sprite is not assigned");
                yield break;
            }

            int totalValue = _currentConfig.endValue - _currentConfig.startValue;
            _remainingItems = poolSize;
            _unitValue = (float)totalValue / poolSize;
            _currentDisplayValue = _currentConfig.startValue;

            // Setup UI
            SetupUI();

            // Spawn all items
            yield return StartCoroutine(SpawnAllItems());

            // Calculate flight path
            Vector2[] curvePoints = GenerateFlightCurve();

            // Animate items to target
            AnimateItemsToTarget(curvePoints);
        }

        private void SetupUI()
        {
            if (realResourceText != null)
            {
                realResourceText.gameObject.SetActive(false);
            }

            if (fakeResourceText != null)
            {
                fakeResourceText.gameObject.SetActive(true);
                fakeResourceText.text = _currentConfig.startValue.ToString();
            }
        }

        private IEnumerator SpawnAllItems()
        {
            bool allItemsSpawned = false;
            int spawnedCount = 0;

            for (int i = 0; i < poolSize; i++)
            {
                int index = i;

                _resourceItems[i].gameObject.SetActive(true);
                _resourceImages[i].sprite = _currentConfig.sprite;
                _resourceImages[i].color = Color.white;

                // Random spawn position
                Vector3 randomOffset = new Vector3(
                    UnityEngine.Random.Range(-100f, 100f),
                    UnityEngine.Random.Range(-100f, 100f),
                    0
                );
                _resourceItems[i].localPosition = randomOffset;
                _resourceItems[i].localScale = Vector3.zero;

                // Scale up animation
                var scaleTween = _resourceItems[i].transform.DOScale(1.2f, scaleUpDuration)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        var scaleDownTween = _resourceItems[index].transform.DOScale(1f, scaleDownDuration)
                            .SetEase(Ease.InOutSine)
                            .OnComplete(() =>
                            {
                                spawnedCount++;
                                if (spawnedCount >= poolSize)
                                {
                                    allItemsSpawned = true;
                                }
                            });
                        tweens.Add(scaleDownTween);
                    });
                tweens.Add(scaleTween);
            }

            // Wait for all items to spawn
            while (!allItemsSpawned)
            {
                yield return new WaitForSeconds(1f / 30f);
            }
        }

        private Vector2[] GenerateFlightCurve()
        {
            Vector2[] curvePoints = new Vector2[5];

            for (int j = 0; j < curvePoints.Length; j++)
            {
                float x = (float)j / (curvePoints.Length - 1);
                float y = flightCurve.Evaluate(x);
                curvePoints[j] = new Vector2(x, y);
            }

            return curvePoints;
        }

        private void AnimateItemsToTarget(Vector2[] curvePoints)
        {
            for (int i = 0; i < poolSize; i++)
            {
                int index = i;

                // Bounce position
                Vector3 bouncePos = _resourceItems[index].localPosition - new Vector3(0, bounceHeight, 0);

                // Create flight path
                Vector3 startPos = _resourceItems[index].transform.position;
                Vector3 distance = _currentConfig.targetPos - startPos;
                Vector3[] path = new Vector3[curvePoints.Length];

                for (int j = 0; j < path.Length; j++)
                {
                    path[j] = startPos + new Vector3(
                        curvePoints[j].x * distance.x,
                        curvePoints[j].y * distance.y
                    );
                }

                // Create animation sequence
                Sequence seq = DOTween.Sequence();

                var moveTween = _resourceItems[index].transform.DOLocalMove(bouncePos, 0.3f)
                    .SetDelay(spawnDelay * index + 0.2f)
                    .SetEase(Ease.InOutSine);

                var pathTween = _resourceItems[index].transform.DOPath(path, flyDuration, PathType.CatmullRom)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() => OnItemReachTarget(index));

                var sizeTween = _resourceItems[index].DOSizeDelta(_currentConfig.targetSize, 0.2f)
                    .SetDelay(0.2f)
                    .SetEase(Ease.InOutSine);

                seq.Append(moveTween);
                seq.Append(pathTween);
                seq.Join(sizeTween);

                tweens.Add(seq);
            }
        }

        private void OnItemReachTarget(int index)
        {
            _remainingItems--;
            _currentDisplayValue += Mathf.RoundToInt(_unitValue);

            // Update UI
            UpdateResourceDisplay();

            // Add punch effect to target
            AddPunchEffect();

            // Play ripple effect
            PlayRippleEffect();

            // Deactivate item
            _resourceItems[index].gameObject.SetActive(false);

            // Call arrive event for each item
            Arrive();

            // Check if animation is complete
            if (_remainingItems <= 0)
            {
                OnResourceAnimationComplete?.Invoke();

                if (realResourceText != null)
                {
                    realResourceText.gameObject.SetActive(true);
                    realResourceText.text = _currentConfig.endValue.ToString();
                }

                if (fakeResourceText != null)
                {
                    fakeResourceText.gameObject.SetActive(false);
                }

                // Call VFXBase complete
                Complete();
            }
        }

        private void UpdateResourceDisplay()
        {
            if (fakeResourceText != null)
            {
                fakeResourceText.text = _currentDisplayValue.ToString();
            }

            OnValueUpdated?.Invoke(_currentDisplayValue);
        }

        private void AddPunchEffect()
        {
            if (_currentConfig.targetTransform == null) return;

            if (_shakeTargetTween != null && !_shakeTargetTween.IsComplete()) return;

            _shakeTargetTween = _currentConfig.targetTransform.DOPunchScale(
                Vector3.one * punchScaleAmount,
                punchDuration
            );

            tweens.Add(_shakeTargetTween);
        }

        private void PlayRippleEffect()
        {
            if (rippleEffect != null && !rippleEffect.isPlaying)
            {
                rippleEffect.transform.position = _currentConfig.targetPos;
                rippleEffect.Play();
            }
        }
        #endregion

        #region Public Methods
        public void SetConfig(ResourceFlyConfig config)
        {
            if (config != null)
            {
                _currentConfig = config;
            }
        }

        public void PlayResourceFly(ResourceFlyConfig config)
        {
            SetConfig(config);
            
            var vfxArgs = new VFXArguments
            {
                amount = config.endValue - config.startValue,
                targetTransform = config.targetTransform,
                worldPos = config.targetPos
            };
            
            Play(vfxArgs);
        }

        public void PlayResourceFly(Sprite sprite, Vector3 targetPos, int startVal, int endVal, string resourceName = "Resource")
        {
            var config = new ResourceFlyConfig
            {
                sprite = sprite,
                targetPos = targetPos,
                startValue = startVal,
                endValue = endVal,
                resourceName = resourceName
            };

            PlayResourceFly(config);
        }
        #endregion

        [Serializable]
        public class ResourceFlyConfig
        {
            public Sprite sprite;
            public Vector3 targetPos;
            public Transform targetTransform;
            public Vector2 targetSize = Vector2.one * 50f;
            public int startValue;
            public int endValue;
            public string resourceName = "Resource";
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(VFXResourceFly))]
    [CanEditMultipleObjects]
    public class VFXResourceFlyEditor : Editor
    {
        private VFXResourceFly script;
        private Texture2D frogIcon;

        // Test fields
        private Sprite testSprite;
        private Vector3 testTargetPos = Vector3.zero;
        private int testStartValue = 100;
        private int testEndValue = 350;

        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (VFXResourceFly)target;

            if (!Application.isPlaying) return;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("VFX Testing", EditorStyles.boldLabel);

            // Test parameters
            testSprite = (Sprite)EditorGUILayout.ObjectField("Test Sprite", testSprite, typeof(Sprite), false);
            testTargetPos = EditorGUILayout.Vector3Field("Target Position", testTargetPos);
            testStartValue = EditorGUILayout.IntField("Start Value", testStartValue);
            testEndValue = EditorGUILayout.IntField("End Value", testEndValue);

            EditorGUILayout.Space(5);

            if (GUILayout.Button(new GUIContent("Test Resource Fly", frogIcon)))
            {
                if (testSprite != null)
                {
                    script.PlayResourceFly(testSprite, testTargetPos, testStartValue, testEndValue);
                }
                else
                {
                    Debug.LogWarning("Please assign a test sprite first!");
                }
            }
        }
    }
    #endif
}