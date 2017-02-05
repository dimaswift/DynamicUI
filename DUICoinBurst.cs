using UnityEngine;
using HandyUtilities;

namespace DynamicUI
{
    public class DUICoinBurst : DUIScreen
    {
        [SerializeField]
        RandomFloatRange m_positionSpread = new RandomFloatRange(10, 100);
        [SerializeField]
        RandomFloatRange m_speedRange = new RandomFloatRange(8, 12);
        [SerializeField]
        RandomFloatRange m_startBurstVeliocity = new RandomFloatRange(8, 12);
        [SerializeField]
        RandomFloatRange m_startDamp = new RandomFloatRange(8, 12);
        [SerializeField]
        RandomFloatRange m_startBurstDuration = new RandomFloatRange(8, 12);
        [SerializeField]
        int m_coinPoolSize = 10;
        [SerializeField]
        RectTransform m_coinPrefab;
        public bool isBursting { get; private set; }

        [SerializeField]
        RectTransform m_coinDestinationRect;

        public delegate void CoinSetter(int c);
        public delegate int CoinGetter();

        CoinSetter m_setter;
        CoinGetter m_getter;

        Coin[] m_coinPool;

        int m_currentReward;
        int m_currentCoinCount;
        int m_currentTargetCoinAmount;

        class Coin
        {
            public bool isActive;
            public float speed;
            public Vector3 position;
            public RectTransform rect;
            public float timer;
            public Vector3 velocity;
            public float damp;
            public float startBurstDuration;
            public bool startBurstEnded;

            public void Update()
            {
                if(!startBurstEnded)
                {
                    rect.position += velocity * Time.deltaTime;
                    velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * damp);
                    startBurstDuration -= Time.deltaTime;

                    if (startBurstDuration <= 0)
                        startBurstEnded = true;
                }

            }
        }

        public override void Init(DUICanvas c)
        {
            base.Init(c);
            m_coinPool = new Coin[m_coinPoolSize];
            for (int i = 0; i < m_coinPoolSize; i++)
            {
                var coin = Instantiate(m_coinPrefab);
                coin.SetParent(transform);
                coin.gameObject.SetActive(false);
                m_coinPool[i] = new Coin() { rect = coin };
            }
            m_coinPrefab.gameObject.SetActive(false);
        }

        public void Burst(int coinCount, Vector3 coinStartPoint, int reward, CoinSetter coinSetter, CoinGetter coinGetter)
        {
            if (isBursting) return;
            isBursting = true;
            m_setter = coinSetter;
            m_getter = coinGetter;
            m_currentReward = reward;
            m_currentTargetCoinAmount = m_getter() + m_currentReward;
            m_currentCoinCount = coinCount;

            var coinEndPoint = m_coinDestinationRect.position;
            m_currentCoinCount = Mathf.Clamp(m_currentCoinCount, 0, m_coinPoolSize);
            for (int i = 0; i < m_currentCoinCount; i++)
            {
                var c = m_coinPool[i];
                c.rect.gameObject.SetActive(true);
                c.rect.position = coinStartPoint;
                c.position = coinStartPoint + new Vector3(m_positionSpread.Get(), m_positionSpread.Get(), 0);
                c.timer = 0;
                c.speed = m_speedRange.Get();
                c.isActive = true;
                c.velocity = new Vector3(m_startBurstVeliocity.Get(), m_startBurstVeliocity.Get());
                c.damp = m_startDamp.Get();
                c.startBurstEnded = false;
                c.startBurstDuration = m_startBurstDuration.Get();
            }
        }

        void Update()
        {
            if(isBursting)
            {
                bool allDone = true;
                for (int i = 0; i < m_coinPool.Length; i++)
                {
                 
                    var coin = m_coinPool[i];
                    if(coin.isActive)
                    {
                        allDone = false;
                        coin.Update();
                        if(coin.startBurstEnded)
                        {
                            coin.rect.position = Vector3.Lerp(coin.rect.position, m_coinDestinationRect.position, Time.deltaTime * coin.speed);
                            if((coin.rect.position - m_coinDestinationRect.position).sqrMagnitude < 1)
                            {
                                coin.rect.gameObject.SetActive(false);
                                coin.isActive = false;
                                m_setter(m_getter() + m_currentReward / m_currentCoinCount);
                            }
                        }
                    }
                }
                if (allDone)
                {
                    m_setter(m_currentTargetCoinAmount);
                    isBursting = false;
                }
            }
        }
    }
}
