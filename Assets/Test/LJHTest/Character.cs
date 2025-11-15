using UnityEngine;
using WarFog;

namespace Test.LJHTest
{
    public class Character : MonoBehaviour
    {
        private WarFogData _warFogData = new(); // todo: 语义上应该是视野数据
        public WarFogData WarFogData => _warFogData;

        private void Awake()
        {
            _warFogData.Init(new[,]
            {
                { WarFogState.Full, WarFogState.Full, WarFogState.Full, WarFogState.Full, WarFogState.Full },
                { WarFogState.Full, WarFogState.None, WarFogState.None, WarFogState.None, WarFogState.Full },
                { WarFogState.Full, WarFogState.None, WarFogState.None, WarFogState.None, WarFogState.Full },
                { WarFogState.Full, WarFogState.None, WarFogState.None, WarFogState.None, WarFogState.Full },
                { WarFogState.Full, WarFogState.Full, WarFogState.Full, WarFogState.Full, WarFogState.Full }
            });

            _warFogData.AddAdjust((Grid) =>
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (i == 0)
                            Grid[i, j] = WarFogState.Partial;
                    }
                }
            });
        }
    }
}
