using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using NamPhuThuy.UI;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NamPhuThuy.VFX
{

    public class VFXCoinFly : VFXBase
    {
        #region Private Serializable Fields

        [SerializeField] private ResourceType resourceType;
        [SerializeField] private TextMeshProUGUI fakeResourceText;
        [SerializeField] private TextMeshProUGUI realResourceText;

        [Header("Stats")]
        [SerializeField] private int poolSize;
        [SerializeField] private Vector3 targetPosition;

        [Header("Components")]
        [SerializeField] private GameObject container;
        [SerializeField] private GameObject coinPanel;

        [Header("VFX")]
        [SerializeField] private GameObject currentVFXPrefab;
        [SerializeField] private Sprite currentVFXSprite;
        [SerializeField] private RectTransform rippleFxCointainer;
        [SerializeField] private ParticleSystem rippleFx;
        #endregion

        #region Private Fields

        [SerializeField] private RectTransform[] _rewards;
        [SerializeField] private Image[] _rewardImages;
        private Transform _coinImage;
        private Vector2 _coinImageSize;
        private SpawnedRewardItemsData _spawnedRewardItemsData;
        private Tweener _shakeFakeResourceTextTween;
        private bool _isRipplePlaying;

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            CreatePool();
        }

        private void Start()
        {
            /*coinPanel = GUIManager.Ins.GUIShop.CoinPanel;
            targetPosition = coinPanel.CoinImage.transform.position;
            realResourceText = coinPanel.CoinText;*/
        }

        #endregion

        #region Private Methods

        protected override void OnPlay()
        {
            KillTweens();
            StartCoroutine(TriggerCollectAnim());
        }

        private void CreatePool()
        {
            _rewards = new RectTransform[poolSize];
            _rewardImages = new Image[poolSize];

            for (int i = 0; i < poolSize; i++)
            {
                _rewards[i] = Instantiate(currentVFXPrefab, container.transform).GetComponent<RectTransform>();
                _rewardImages[i] = _rewards[i].GetComponent<Image>();

                _rewards[i].gameObject.SetActive(false);
            }
        }

        private IEnumerator TriggerCollectAnim()
        {
            /*var data = VFXManager.Ins.ResourceCollectVFXDatas.GetVFXData(resourceType);
            if (data == null) yield break;

            // Debug.Log($"Check here");
            currentVFXSprite = data.resourceSprite;*/

            int totalValue = args.amount;
            // int itemSizeX = (int)(0.12f * GamePersistentVariable.canvasSize.x);

            bool isAllCoinSpawned = false;

            for (int i = 0; i < poolSize; i++)
            {
                int index = i;

                // Debug.Log($"first id: {i}");
                _rewards[i].gameObject.SetActive(true);

                /*itemSizeX = (int)(Random.Range(0.11f, 0.13f) * GamePersistentVariable.canvasSize.x);

                // Debug.Log($"second id: {i}");
                UIUtil.SetSizeKeepRatioY(_rewards[i], itemSizeX);*/

                _rewardImages[i].sprite = currentVFXSprite;
                _rewardImages[i].color = Color.white;
                _rewards[i].localPosition = new Vector3(/*Random.Range(-2 * itemSizeX, 2 * itemSizeX), Random.Range(-2 * itemSizeX, 2 * itemSizeX)*/);
                // _rewards[i].localPosition = Vector3.zero;
                _rewards[i].localScale = Vector3.zero;

                _rewards[i].transform.DOScale(1.2f, 0.3f).SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    _rewards[index].transform.DOScale(1f, 0.2f).SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        if (index == poolSize - 1)
                        {
                            isAllCoinSpawned = true;
                        }
                    });
                });
            }

            while (!isAllCoinSpawned)
            {
                yield return YieldHelper.Get(1f / 30);
            }

            float remainValue = totalValue % poolSize;
            float unitValue = (totalValue - remainValue) / poolSize;

            _spawnedRewardItemsData = new SpawnedRewardItemsData();

            _spawnedRewardItemsData.totalItems = poolSize;
            _spawnedRewardItemsData.remaningItems = poolSize;
            _spawnedRewardItemsData.unitValue = unitValue;

            AutoFindResourceDisplay();

            realResourceText.gameObject.SetActive(false);
            fakeResourceText.gameObject.SetActive(true);

            fakeResourceText.text = $"{DataManager.Ins.PlayerData.Coin - totalValue}";

            Vector2[] curvePoints = new Vector2[5];

            for (int j = 0; j < curvePoints.Length; j++)
            {
                float x = (float)j / (curvePoints.Length - 1);
                float y = EvaluateSaturationCurve(x, 8);

                curvePoints[j] = new Vector2(x, y);
            }

            for (int i = 0; i < poolSize; i++)
            {
                int index = i;

                Vector3 randomBouncePosition = _rewards[index].localPosition - new Vector3(0, Random.Range(100, 200), 0);

                Sequence seq = DOTween.Sequence();

                Vector3 startPosition = _rewards[index].transform.position;
                Vector3 distance = targetPosition - startPosition;

                Vector3[] path = new Vector3[5];

                for (int j = 0; j < path.Length; j++)
                {
                    path[j] = startPosition + new Vector3(curvePoints[j].x * distance.x, curvePoints[j].y * distance.y);
                }

                seq.Append(_rewards[index].transform.DOLocalMove(randomBouncePosition, 0.3f).SetDelay(0.12f * index + 0.2f).SetEase(Ease.InOutSine));
                seq.Append(_rewards[index].transform.DOPath(path, duration: 0.4f, pathType: PathType.CatmullRom).SetEase(Ease.InOutSine)).OnComplete(() =>
                {
                    float valueLeft = (poolSize - index - 1) * unitValue;

                    if (index == poolSize - 1)
                    {
                        valueLeft = 0;
                    }

                    _spawnedRewardItemsData.remaningItems--;

                    if (_spawnedRewardItemsData.remaningItems <= 0)
                    {
                        realResourceText.gameObject.SetActive(true);
                        fakeResourceText.gameObject.SetActive(false);

                        fakeResourceText.transform.SetParent(transform);

                        realResourceText.text = $"{DataManager.Ins.PlayerData.Coin}";
                    }
                    else
                    {
                        if (_shakeFakeResourceTextTween != null)
                        {
                            if (_shakeFakeResourceTextTween.IsComplete())
                            {
                                _shakeFakeResourceTextTween = _coinImage.DOPunchScale(0.15f * Vector3.one, 0.3f);
                            }
                        }
                        else
                        {
                            _shakeFakeResourceTextTween = _coinImage.DOPunchScale(0.15f * Vector3.one, 0.3f);
                        }

                        fakeResourceText.text = $"{DataManager.Ins.PlayerData.Coin - _spawnedRewardItemsData.remaningItems * _spawnedRewardItemsData.unitValue}";
                    }

                    _rewards[index].gameObject.SetActive(false);

                    // if (!rippleFx.isPlaying)
                    // {
                    //     rippleFxCointainer.transform.position = targetPosition;
                    //     rippleFx.Play();
                    // }

                });
                seq.Join(_rewards[index].DOSizeDelta(_coinImageSize, duration: 0.2f).SetDelay(0.2f)).SetEase(Ease.InOutSine);
            }

            // Exponential saturation curve: y = maxY * (1 - e^(-k * x))
            float EvaluateSaturationCurve(float x, float k, float yMax = 1)
            {
                return yMax * (1f - Mathf.Exp(-k * x));
            }
        }

        private void AutoFindResourceDisplay()
        {
            // coinPanel = CommonUtils.GetLastActiveOfType<CoinPanel>();
            /*targetPosition = coinPanel.CoinImage.transform.position;
            realResourceText = coinPanel.CoinText;*/

            // _coinImage = coinPanel.CoinImage.transform;
            _coinImageSize = _coinImage.GetComponent<RectTransform>().sizeDelta * _coinImage.transform.lossyScale.x;

            // CommonUtils.CopyTextStyle(fakeResourceText, realResourceText);
        }

        #endregion

        #region Public Methods
        #endregion
    }
}
