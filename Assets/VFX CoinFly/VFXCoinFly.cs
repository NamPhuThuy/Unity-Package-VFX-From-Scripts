using System;
using System.Collections;
using DG.Tweening;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace NamPhuThuy.VFX
{
    public class VFXCoinFly : VFXBase
    {
        private const int CURVE_POINT_COUNT = 5;
        private const float CURVE_STRENGTH = 8f;
        private const float SPAWN_DELAY = 0.12f;
        private const float INITIAL_DELAY = 0.2f;
        private const float BOUNCE_MIN = 100f;
        private const float BOUNCE_MAX = 200f;
        private const float SCALE_MIN = 0.8f;
        private const float SCALE_MAX = 1.2f;
        private const float SIZE_RANDOM_MIN = 1.1f;
        private const float SIZE_RANDOM_MAX = 1.3f;


        [Header("Stats")]
        
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private int totalAmount;
        [SerializeField] private int prevValue;

        [Header("Components")]
        [SerializeField] private GameObject container;
        [SerializeField] private TextMeshProUGUI fakeResourceText;
        [SerializeField] private TextMeshProUGUI realResourceText;
        [SerializeField] private Transform targetResourceImage;

        [Header("VFX")]
        [SerializeField] private Sprite currentVFXSprite;
        [SerializeField] private GameObject currentIconPrefab;
        [SerializeField] private RectTransform[] iconList;
       
        [SerializeField] private RectTransform rippleFxCointainer;
        [SerializeField] private ParticleSystem rippleFx;
       
        private Tweener _shakeFakeResourceTextTween;

        #region Private Fields

        private readonly int _poolSize = 8;
        private int _unitValue;
        private int _remainingItems;
        private CoinFlyArgs _currentArgs;
        
        
        private Transform _initTextParent;
        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            CreatePool();
            _initTextParent = fakeResourceText.transform.parent;
            // args.onBegin += SetValues;
        }

        #endregion

        protected override void OnPlay()
        {
            SetValues();
            KillTweens();
            StartCoroutine(TriggerCollectAnim());
        }

        #region Override Methods

        public override void Play<T>(T args)
        {
            if (args is CoinFlyArgs coinArgs)
            {
                PlayCoinFly(coinArgs);
            }
        }
        
        private void PlayCoinFly(CoinFlyArgs coinArgs)
        {
            _currentArgs = coinArgs;
            gameObject.SetActive(true);
            SetValues();
            KillTweens();
            StartCoroutine(TriggerCollectAnim());
        }

        public override void Play(object args)
        {
            throw new NotImplementedException();
        }
        

        #endregion

        #region Set up

        private void SetValues()
        {
            /*realResourceText = args.targetTransform.GetComponent<TextMeshProUGUI>();
            targetPosition = args.interactTransform.position;
            
            totalAmount = args.amount;
            prevValue = args.prevAmount;

            _remainingItems = _poolSize;
            _unitValue = totalAmount / _poolSize;*/
            
            // NEW
            realResourceText = _currentArgs.target.GetComponent<TextMeshProUGUI>();
            targetPosition = _currentArgs.interactTarget ? _currentArgs.interactTarget.position : _currentArgs.target.position;
        
            totalAmount = _currentArgs.amount;
            prevValue = _currentArgs.prevAmount;

            _remainingItems = _poolSize;
            _unitValue = totalAmount / _poolSize;
            transform.position = _currentArgs.startPosition;
        }
        
        private void CreatePool()
        {
            iconList = new RectTransform[_poolSize];

            for (int i = 0; i < _poolSize; i++)
            {
                var icon = Instantiate(currentIconPrefab, args.worldPos, Quaternion.identity).GetComponent<RectTransform>();
                icon.SetParent(container.transform, true);
                icon.GetComponent<Image>().SetNativeSize();

                iconList[i] = icon;
                icon.gameObject.SetActive(false);
            }
        }

        #endregion

        private IEnumerator TriggerCollectAnim()
        {
            int itemSizeX = currentVFXSprite.texture.width;
            bool isAllCoinSpawned = false;

            for (int i = 0; i < _poolSize; i++)
            {
                SetupRewardItem(i, itemSizeX, () => isAllCoinSpawned = true, i == _poolSize - 1);
            }

            while (!isAllCoinSpawned)
                yield return YieldHelper.Get(1f / 30);

            AutoFindResourceDisplay();

            realResourceText.gameObject.SetActive(false);
            fakeResourceText.gameObject.SetActive(true);
            fakeResourceText.text = prevValue.ToString();

            for (int i = 0; i < _poolSize; i++)
            {
                var curvePoints = GenerateCurvePoints(i);
                AnimateRewardItem(i, curvePoints);
            }
        }

        // Change SetupRewardItem signature:
        private void SetupRewardItem(int index, int itemSizeX, System.Action onLastItem, bool isLast)
        {
            int randomSizeX = (int)(Random.Range(SIZE_RANDOM_MIN, SIZE_RANDOM_MAX) * itemSizeX);
            var reward = iconList[index];
            Image image = reward.GetComponent<Image>();

            reward.gameObject.SetActive(true);
            image.SetSizeKeepRatioY(randomSizeX);
            image.sprite = currentVFXSprite;
            image.color = Color.white;
            
            reward.localPosition = new Vector3(Random.Range(-2 * itemSizeX, 2 * itemSizeX), Random.Range(-2 * itemSizeX, 2 * itemSizeX));
            reward.localScale = Vector3.zero;

            float randomScale = Random.Range(SCALE_MIN, SCALE_MAX);

            var sequence = DOTween.Sequence();
            sequence.Append(reward.transform.DOScale(randomScale * 1.2f, 0.3f).SetEase(Ease.InOutSine));
            sequence.Append(reward.transform.DOScale(randomScale, 0.2f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                if (isLast)
                    onLastItem?.Invoke();
            }));
        }

        private void AnimateRewardItem(int index, Vector2[] curvePoints)
        {
            var reward = iconList[index];
            var startPosition = reward.transform.position;
            var distance = targetPosition - startPosition;

            var path = new Vector3[CURVE_POINT_COUNT];
            for (int j = 0; j < CURVE_POINT_COUNT; j++)
                path[j] = startPosition + new Vector3(curvePoints[j].x * distance.x, curvePoints[j].y * distance.y);

            var randomBouncePosition = reward.localPosition - new Vector3(0, Random.Range(BOUNCE_MIN, BOUNCE_MAX), 0);

            var seq = DOTween.Sequence();
            seq.Append(reward.transform.DOLocalMove(randomBouncePosition, 0.3f).SetDelay(SPAWN_DELAY * index + INITIAL_DELAY).SetEase(Ease.InOutSine));
            seq.Append(reward.transform.DOPath(path, 0.4f, PathType.CatmullRom).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                _remainingItems--;

                if (_remainingItems <= 0)
                {
                    realResourceText.gameObject.SetActive(true);
                    fakeResourceText.gameObject.SetActive(false);
                    fakeResourceText.transform.SetParent(transform);
                    realResourceText.text = $"{prevValue + totalAmount}";
                    
                    // NEW
                    _currentArgs.onArrive?.Invoke();
                    _currentArgs.onComplete?.Invoke();
                }
                else
                {
                    UpdateFakeResourceText();
                }

                reward.gameObject.SetActive(false);
            }));
            // seq.Join(reward.DOSizeDelta(_coinImageSize, 0.2f).SetDelay(0.2f)).SetEase(Ease.InOutSine);
        }

        private void UpdateFakeResourceText()
        {
            if (_shakeFakeResourceTextTween != null && _shakeFakeResourceTextTween.IsComplete())
            {
                // _shakeFakeResourceTextTween = _coinImage.DOPunchScale(0.15f * Vector3.one, 0.3f);
            }
            else
            {
                // _shakeFakeResourceTextTween = _coinImage.DOPunchScale(0.15f * Vector3.one, 0.3f);
            }

            fakeResourceText.text = $"{prevValue + totalAmount - _remainingItems * _unitValue}";
        }

        private enum CurveType
        {
            Exponential,
            Sine,
            Parabolic,
            Linear
        }
        
        private Vector2[] GenerateCurvePoints(int coinIndex)
        {
            /*var points = new Vector2[CURVE_POINT_COUNT];
            for (int j = 0; j < CURVE_POINT_COUNT; j++)
            {
                float x = (float)j / (CURVE_POINT_COUNT - 1);
                float y = EvaluateSaturationCurve(x, CURVE_STRENGTH);
                points[j] = new Vector2(x, y);
            }
            return points;*/
            
            var points = new Vector2[CURVE_POINT_COUNT];
    
            // Create different curve types based on coin index
            CurveType curveType = (CurveType)(coinIndex % 4);
    
            for (int j = 0; j < CURVE_POINT_COUNT; j++)
            {
                float x = (float)j / (CURVE_POINT_COUNT - 1);
                float y = 0f;
        
                switch (curveType)
                {
                    case CurveType.Exponential:
                        y = EvaluateSaturationCurve(x, CURVE_STRENGTH);
                        break;
                    case CurveType.Sine:
                        y = Mathf.Sin(x * Mathf.PI * 0.5f) * 1.2f; // Arc shape
                        break;
                    case CurveType.Parabolic:
                        y = x * x * 1.5f; // Steeper at end
                        break;
                    case CurveType.Linear:
                        y = x; // Straight line
                        break;
                }
        
                // Add some randomness to each point
                float randomOffset = Random.Range(-0.1f, 0.1f);
                y = Mathf.Clamp01(y + randomOffset);
        
                points[j] = new Vector2(x, y);
            }
            return points;
        }

        // Exponential saturation curve: y = maxY * (1 - e^(-k * x))
        private float EvaluateSaturationCurve(float x, float k, float yMax = 1f)
        {
            return yMax * (1f - Mathf.Exp(-k * x));
        }

        private void AutoFindResourceDisplay()
        {
            if (realResourceText == null) return;
    
            fakeResourceText.CopyProperties(realResourceText);
            fakeResourceText.transform.SetParent(realResourceText.transform.parent);
            fakeResourceText.rectTransform.localPosition = realResourceText.rectTransform.localPosition;
            fakeResourceText.rectTransform.sizeDelta = realResourceText.rectTransform.sizeDelta;
            fakeResourceText.transform.localScale = realResourceText.transform.localScale;
        }
    }
}