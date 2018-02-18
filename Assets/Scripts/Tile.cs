using UnityEngine;

namespace Assets.Scripts
{
    public class Tile : MonoBehaviour
    {
        public int Row;
        public int Col;
        public int? Stack = null;
        public string Color { get; set; }
    }
}
