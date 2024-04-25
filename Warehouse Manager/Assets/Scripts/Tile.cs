using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField]
    TileTypes tileTypes;
    [SerializeField]
    MeshRenderer meshRenderer;
    public void ChangeTileType(float noiseSample)
    {
        for(int i = 0;i<tileTypes.tileTypesRanges.Count;i++)
        {
            if(noiseSample >= tileTypes.tileTypesRanges[i].tileRange.x && noiseSample < tileTypes.tileTypesRanges[i].tileRange.y)
            {
                Material material = meshRenderer.material;

                
                material.color = tileTypes.tileTypesRanges[i].color;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Button button = other.GetComponent<Button>();

        if (button != null)
        {
            other.gameObject.SetActive(false);
        }
    }
}
