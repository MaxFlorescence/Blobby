using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     A class defining the minimap of the game.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class MiniMap : MonoBehaviour
{
    // ---------------------------------------------------------------------------------------------
    // GRAPHICS
    // ---------------------------------------------------------------------------------------------
    private Material MAP_ICON_MATERIAL;
    public GameObject playerIcon;
    public GameObject mapBorder;
    /// <summary>
    ///     The canvas on which minimap icons are drawn.
    /// </summary>
    public GameObject tileCanvas;

    // ---------------------------------------------------------------------------------------------
    // DIMENSIONS
    // ---------------------------------------------------------------------------------------------
    private RectTransform rectTransform;
    /// <summary>
    ///     The position of the minimap's center point on the screen, with inverted y coordinate.
    /// </summary>
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

        MAP_ICON_MATERIAL = Resources.Load<Material>(
            Path.Combine(FileUtilities.BASIC_MATERIALS, "Minimap Icon")
        );
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

    /// <summary>
    ///     Creates a new minimap icon gameobject with the corresponding parameters.
    /// </summary>
    /// <param name="name">
    ///     The name of the minimap icon.
    /// </param>
    /// <param name="sprite">
    ///     The sprite to use for the minimap icon.
    /// </param>
    /// <param name="position">
    ///     The position of the icon on the minimap, as if it were a 1x1 square with origin at its
    ///     center point.
    /// </param>
    /// <param name="rotation">
    ///     The rotation of the icon on the minimap.
    /// </param>
    /// <param name="scale">
    ///     The scale of the icon on the minimap.
    /// </param>
    /// <returns>
    ///     The newly created gameobject.
    /// </returns>
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

    /// <param name="posX">
    ///     The horizontal position between -0.5 and 0.5.
    /// </param>
    /// <param name="posY">
    ///     The vertical position between -0.5 and 0.5.
    /// </param>
    /// <returns>
    ///     The given (x,y) position on the minimap as a local position.
    /// </returns>
    private Vector3 MapPosition(float posX = 0, float posY = 0)
    {
        Rect mapRect = rectTransform.rect;
        return new(posX * mapRect.width, posY * mapRect.height, 0);
    }
}