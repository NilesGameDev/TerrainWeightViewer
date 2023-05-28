using FlaxEditor;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game
{
    /// <summary>
    /// TerrainUtils Script.
    /// </summary>
    public class TerrainUtils : Script
    {
        public UIControl SplatMapInfoTextBoxUI;
        public UIControl HitTextBoxUI;

        private Int2 _defaultPatchCoord = new Int2(0, 0); // assume that terrain patch coord is (0,0)

        private Terrain _terrain;
        private TextBox _splatMapInfoTextBox;
        private TextBox _hitTextBox;

        private int _splatMapSize;
        private int _terrainHeight; // as world unit, as centimeter
        private int _terrainWidth; // as world unit, as centimeter

        /// <inheritdoc/>
        public override void OnStart()
        {
            _terrain = Actor.As<Terrain>();
            if (SplatMapInfoTextBoxUI != null)
            {
                _splatMapInfoTextBox = SplatMapInfoTextBoxUI.Get<TextBox>();
            }
            if (HitTextBoxUI != null)
            {
                _hitTextBox = HitTextBoxUI.Get<TextBox>();
            }

            _splatMapSize = _terrain.ChunkSize * Terrain.PatchEdgeChunksCount + 1;
            _terrainWidth = _terrainHeight = _terrain.ChunkSize * Terrain.PatchEdgeChunksCount * 100; // convert to world unit
        }

        /// <inheritdoc/>
        public override void OnUpdate()
        {
            var mousePos = Input.MousePosition;
            Ray ray = Camera.MainCamera.ConvertMouseToRay(mousePos);

            if (Physics.RayCast(ray.Position, ray.Direction, out var hit))
            {
                var hitPoint = hit.Point;
                var actualHitPosition = hitPoint - _terrain.Position;
                var splatMapPos = new Vector2(
                    actualHitPosition.X / _terrainWidth * _splatMapSize,
                    actualHitPosition.Z / _terrainHeight * _splatMapSize
                );

                int x = Mathf.FloorToInt(splatMapPos.X);
                int z = Mathf.FloorToInt(splatMapPos.Y);
                var splatMapDataPoint = GetSplatMapDataAtPoint(x, z);
                if (_hitTextBox != null)
                {
                    _hitTextBox.SetText(hitPoint.ToString());
                }
                if (_splatMapInfoTextBox != null)
                {
                    _splatMapInfoTextBox.SetText($"X: {x}, Z: {z} -\tWeight: {splatMapDataPoint}");
                }
            }
        }

        private Color32 GetSplatMapDataAtPoint(int x, int z)
        {
            var splatMapData = default(Color32);
            unsafe
            {
                Color32* splatMapPtr = TerrainTools.GetSplatMapData(_terrain, ref _defaultPatchCoord, 0);
                if (splatMapPtr != null)
                {
                    splatMapData = splatMapPtr[z * _splatMapSize + x];
                }
            }

            return splatMapData;
        }
    }
}
