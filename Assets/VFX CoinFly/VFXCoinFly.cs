using System.Collections;
using DG.Tweening;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField] private ResourceType resourceType;
        [SerializeField] private TextMeshProUGUI fakeResourceText;
        [SerializeField] private TextMeshProUGUI realResourceText;

        [Header("Stats")]
        [SerializeField] private int poolSize;
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private int totalAmount;
        [SerializeField] private int prevValue;

        [Header("Components")]
        [SerializeField] private GameObject container;

        [Header("VFX")]
        [SerializeField] private GameObject currentVFXPrefab;
        [SerializeField] private Sprite currentVFXSprite;
        [SerializeField] private RectTransform rippleFxCointainer;
        [SerializeField] private ParticleSystem rippleFx;

        [SerializeField] private RectTransform[] _rewards;
        [SerializeField] private Image[] _rewardImages;
        [SerializeField] private Transform _coinImage;
        private Vector2 _coinImageSize;
        private SpawnedRewardItemsData _spawnedRewardItemsData;
        private Tweener _shakeFakeResourceTextTween;

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            CreatePool();
        }

        #endregion

        protected override void OnPlay()
        {
            SetValues();
            KillTweens();
            StartCoroutine(TriggerCollectAnim());
        }

        

        #region Set up

        private void SetValues()
        {
            realResourceText = args.target.GetComponent<TextMeshProUGUI>();
            totalAmount = args.amount;
            prevValue = args.prevAmount;
        }
        
        private void CreatePool()
        {
            _rewards = new RectTransform[poolSize];
            _rewardImages = new Image[poolSize];

            for (int i = 0; i < poolSize; i++)
            {
                var reward = Instantiate(currentVFXPrefab, args.worldPos, Quaternion.identity).GetComponent<RectTransform>();
                reward.SetParent(container.transform, true);
                
                var image = reward.GetComponent<Image>();
                image.SetNativeSize();

                _rewards[i] = reward;
                _rewardImages[i] = image;
                reward.gameObject.SetActive(false);
            }
        }

        #endregion

        private IEnumerator TriggerCollectAnim()
        {
            int itemSizeX = currentVFXSprite.texture.width;
            bool isAllCoinSpawned = false;

            for (int i = 0; i < poolSize; i++)
            {
                SetupRewardItem(i, itemSizeX, () => isAllCoinSpawned = true, i == poolSize - 1);
            }

            while (!isAllCoinSpawned)
                yield return YieldHelper.Get(1f / 30);

            float remainValue = totalAmount % poolSize;
            float unitValue = (totalAmount - remainValue) / poolSize;

            _spawnedRewardItemsData = new SpawnedRewardItemsData
            {
                totalItems = poolSize,
                remaningItems = poolSize,
                unitValue = unitValue
            };

            AutoFindResourceDisplay();

            realResourceText.gameObject.SetActive(false);
            fakeResourceText.gameObject.SetActive(true);
            fakeResourceText.text = "0";

            var curvePoints = GenerateCurvePoints();

            for (int i = 0; i < poolSize; i++)
            {
                AnimateRewardItem(i, curvePoints, unitValue);
            }
        }

        // Change SetupRewardItem signature:
        private void SetupRewardItem(int index, int itemSizeX, System.Action onLastItem, bool isLast)
        {
            var reward = _rewards[index];
            var image = _rewardImages[index];

            reward.gameObject.SetActive(true);
            int randomSizeX = (int)(Random.Range(SIZE_RANDOM_MIN, SIZE_RANDOM_MAX) * itemSizeX);
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

        private void AnimateRewardItem(int index, Vector2[] curvePoints, float unitValue)
        {
            var reward = _rewards[index];
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
                _spawnedRewardItemsData.remaningItems--;

                if (_spawnedRewardItemsData.remaningItems <= 0)
                {
                    realResourceText.gameObject.SetActive(true);
                    fakeResourceText.gameObject.SetActive(false);
                    fakeResourceText.transform.SetParent(transform);
                    realResourceText.text = $"{prevValue + totalAmount}";
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

            fakeResourceText.text = $"{prevValue + totalAmount - _spawnedRewardItemsData.remaningItems * _spawnedRewardItemsData.unitValue}";
        }

        private Vector2[] GenerateCurvePoints()
        {
            var points = new Vector2[CURVE_POINT_COUNT];
            for (int j = 0; j < CURVE_POINT_COUNT; j++)
            {
                float x = (float)j / (CURVE_POINT_COUNT - 1);
                float y = EvaluateSaturationCurve(x, CURVE_STRENGTH);
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

            targetPosition = realResourceText.transform.position;
            _coinImageSize = realResourceText.rectTransform.sizeDelta;
    
            // Copy text style from real to fake text
            fakeResourceText.CopyProperties(realResourceText);
            fakeResourceText.transform.SetParent(realResourceText.transform.parent, true);
            fakeResourceText.transform.position = realResourceText.transform.position;
            
            /*fakeResourceText.fontSize = realResourceText.fontSize;
            fakeResourceText.font = realResourceText.font;
            fakeResourceText.color = realResourceText.color;*/
        }
    }
}