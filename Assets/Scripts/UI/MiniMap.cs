using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class MiniMap : MonoBehaviour
{
    public GameObject playerIcon;
    public GameObject tileCanvas;
    public GameObject mapBorder;

    private Material MAP_ICON_MATERIAL;
    private RectTransform rectTransform;
    private Vector3 mapScreenPosition;

    void Awake()
    {
        GameInfo.ActiveMiniMap = this;
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);
        mapScreenPosition = (worldCorners[0] + worldCorners[2]) / 2;
        mapScreenPosition.y = Screen.height - mapScreenPosition.y;

        MAP_ICON_MATERIAL = Resources.Load("Materials/Basic Materials/Minimap Icon", typeof(Material)) as Material;
        MAP_ICON_MATERIAL.SetVector("_Center", mapScreenPosition);
        MAP_ICON_MATERIAL.SetFloat("_Radius", MapPosition(0.48f, 0.48f).Min(true));
    }

    void Update()
    {
        Vector3 dungeonPosition = GameInfo.CurrentDungeon.CoordinatesOf(
            GameInfo.ControlledBlob.GetPosition()
        );
        dungeonPosition = GameInfo.CurrentDungeon.TransformPosition(
            dungeonPosition, false, true, true
        );

        Quaternion viewRotation = Quaternion.Euler(0, 0,
            GameInfo.ControlledCamera.transform.rotation.eulerAngles.y
        );

        RectTransform tileRect = tileCanvas.GetComponent<RectTransform>();
        tileRect.pivot = new(dungeonPosition.x, dungeonPosition.z);
        tileRect.rotation = viewRotation;
        mapBorder.GetComponent<RectTransform>().rotation = viewRotation;
    }

    public GameObject AddIcon(string name, Sprite sprite, Vector3 position, float rotation, float scale)
    {
        GameObject mapIcon = new(name);
        mapIcon.transform.SetParent(tileCanvas.transform);

        Image image = mapIcon.AddComponent<Image>();
        image.sprite = sprite;
        image.material = MAP_ICON_MATERIAL;
    
        RectTransform iconTransform = mapIcon.GetComponent<RectTransform>();
        iconTransform.SetLocalPositionAndRotation(
            MapPosition(position.x, position.z), Quaternion.Euler(0, 0, rotation)
        );
        iconTransform.localScale = scale * Vector3.one;

        return mapIcon;
    }

    // -0.5 <= posX,Y <= 0.5
    private Vector3 MapPosition(float posX = 0, float posY = 0)
    {
        Rect mapRect = rectTransform.rect;
        return new(posX * mapRect.width, posY * mapRect.height, 0);
    }
}