/*
Github: https://github.com/NamPhuThuy
*/

using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace NamPhuThuy.AnimateWithScripts
{
    public class VFXItemFly : VFXBase
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
        [SerializeField] private GameObject itemContainer;
        [SerializeField] private TextMeshProUGUI fakeResourceText;
        [SerializeField] private TextMeshProUGUI realResourceText;
        [SerializeField] private Transform targetInteractTransform;

        [Header("VFX")]
        [SerializeField] private Sprite itemSprite;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private RectTransform[] iconList;
       
        [SerializeField] private RectTransform rippleFxCointainer;
        [SerializeField] private ParticleSystem rippleFx;
       
        private Tweener _shakeFakeResourceTextTween;

        #region Private Fields

        private readonly int _poolSize = 8;
        private int _unitValue;
        private int _remainingItems;
        private ItemFlyArgs _currentArgs;
        
        
        private Transform _initTextParent;
        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            CreatePool();
            _initTextParent = fakeResourceText.transform.parent;
        }

        #endregion

        #region Override Methods

        public override void Play<T>(T args)
        {
            if (args is ItemFlyArgs coinArgs)
            {
                _currentArgs = coinArgs;
                gameObject.SetActive(true);
                SetValues();
                KillTweens();
                StartCoroutine(TriggerCollectAnim());
            }
        }
        
        public override void Play(object args)
        {
            throw new NotImplementedException();
        }
        

        #endregion

        #region Set up

        private void SetValues()
        {
            realResourceText = _currentArgs.target.GetComponent<TextMeshProUGUI>();
            targetPosition = _currentArgs.targetInteractTransform ? _currentArgs.targetInteractTransform.transform.position : _currentArgs.target.position;
            
            targetInteractTransform = _currentArgs.targetInteractTransform ? _currentArgs.targetInteractTransform : null;
            
            itemSprite = _currentArgs.itemSprite ?? itemSprite;
        
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
                var item = Instantiate(itemPrefab, transform.position, Quaternion.identity).GetComponent<RectTransform>();
                item.SetParent(itemContainer.transform, true);
                item.GetComponent<Image>().SetNativeSize();

                iconList[i] = item;
                item.gameObject.SetActive(false);
            }
        }

        #endregion

        private IEnumerator TriggerCollectAnim()
        {
            int itemSizeX = itemSprite.texture.width;
            bool isAllCoinSpawned = false;

            for (int i = 0; i < _poolSize; i++)
            {
                SetupRewardItem(i, itemSizeX, () => isAllCoinSpawned = true, i == _poolSize - 1);
            }

            while (!isAllCoinSpawned)
                yield return new WaitForSeconds(1f / 30);

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
            image.sprite = itemSprite;
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
                    _currentArgs.onComplete?.Invoke();
                }
                else
                {
                    _currentArgs.onItemInteract?.Invoke();
                    UpdateFakeResourceText();
                }

                reward.gameObject.SetActive(false);
            }));
        }

        private void UpdateFakeResourceText()
        {
            if (_shakeFakeResourceTextTween != null && _shakeFakeResourceTextTween.IsActive())
            {
                _shakeFakeResourceTextTween.Kill();
            }

            targetInteractTransform.localScale = Vector3.one;

            _shakeFakeResourceTextTween = targetInteractTransform
                .DOPunchScale(0.15f * Vector3.one, 0.3f);

            fakeResourceText.text = $"{prevValue + totalAmount - _remainingItems * _unitValue}";
        }

        private enum CurveType
        {
            EXPONENTIAL = 0,
            SINE = 1,
            PARABOLIC = 2,
            LINEAR = 3,
            LOGARITHMIC = 4,
            // BOUNCE = 5,
            /*ZIGZAG = 6,
            CIRCULAR = 7*/
        }
        
        private Vector2[] GenerateCurvePoints(int coinIndex)
        {
            var points = new Vector2[CURVE_POINT_COUNT];
    
            // Create different curve types based on coin index
            CurveType curveType = (CurveType)(coinIndex % Enum.GetValues(typeof(CurveType)).Length);
    
            for (int j = 0; j < CURVE_POINT_COUNT; j++)
            {
                float x = (float)j / (CURVE_POINT_COUNT - 1);
                float y = 0f;
        
                switch (curveType)
                {
                    case CurveType.EXPONENTIAL:
                        y = EvaluateSaturationCurve(x, CURVE_STRENGTH);
                        break;
                    case CurveType.SINE:
                        y = Mathf.Sin(x * Mathf.PI * 0.5f) * 1.2f; // Arc shape
                        break;
                    case CurveType.PARABOLIC:
                        y = x * x * 1.5f; // Steeper at end
                        break;
                    case CurveType.LINEAR:
                        y = x; // Straight line
                        break;
                    case CurveType.LOGARITHMIC:
                        y = Mathf.Log10(1 + 9 * x); // log curve, starts slow, ends fast
                        break;
                    /*case CurveType.BOUNCE:
                        y = Mathf.Abs(Mathf.Sin(3 * Mathf.PI * x)) * (1 - x); // bouncy effect
                        break;
                    case CurveType.ZIGZAG:
                        y = (j % 2 == 0) ? 0.2f : 0.8f; // sharp zigzag
                        break;
                    case CurveType.CIRCULAR:
                        y = 1 - Mathf.Sqrt(1 - x * x); // quarter circle*/
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