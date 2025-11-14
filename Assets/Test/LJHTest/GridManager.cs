using System.Collections.Generic;
using UnityEngine;
using WarFog;

namespace Test.LJHTest
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField]private List<Grid> grids = new();
        private const int X = 5;
        private const int Y = 5;
        
        public Grid this[int x, int y] => grids[x * Y + y];
        
        public Character character;

        private void Start()
        {
            UpdateWarFog(character.WarFogData.VisibilityGrids);
        }

        public Grid GetGrid(int x, int y)
        {
            return grids[x * Y + y];
        }
        
        public void UpdateWarFog(WarFogState[,] states)
        {
            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    GetGrid(i, j).warFog.SetWarFogState(states[i, j]);
                }
            }
        }
    }
}
