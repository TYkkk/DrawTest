using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public SpriteRenderer srd;
    public int width = 3;
    public Color penColor;
    public Color lineColor = Color.black;

    public bool isMirrorMode = false;

    public Slider WidthSlider;
    public Button SelectColorBtn;
    public ColorPanelControl ColorPanel;
    public Toggle MirrorSwitch;

    private Camera camera;
    private Color[] srdColor;

    private int index;
    private bool isFirst = true;
    private Vector2 previousPos;
    private Vector2 currentPos;
    private Color[] initColor;

    private void Awake()
    {
        camera = Camera.main;
        srdColor = srd.sprite.texture.GetPixels();
        ClearColor();
        WidthSlider.value = width;
        WidthSlider.onValueChanged.AddListener(WidthSliderValueChangedEvent);
        SelectColorBtn.onClick.AddListener(SelectColorBtnClicked);
        ColorPanel.SetTargetColor(penColor);
        ColorPanel.SelectedEvent = SelectColorEvent;
        SelectColorBtn.image.color = penColor;
        MirrorSwitch.isOn = isMirrorMode;
        MirrorSwitch.onValueChanged.AddListener(MirrorSwitchChangedEvent);
    }

    public void SelectColorEvent(Color color)
    {
        penColor = color;
        SelectColorBtn.image.color = color;
    }

    private void MirrorSwitchChangedEvent(bool ison)
    {
        isMirrorMode = ison;
    }

    private void SelectColorBtnClicked()
    {
        ColorPanel.gameObject.SetActive(true);
    }

    private void InitCanvasColor()
    {
        initColor = new Color[srdColor.Length];
        for (int i = 0; i < initColor.Length; i++)
        {
            initColor[i] = Color.white;
        }

        srd.sprite.texture.SetPixels(initColor);
        srd.sprite.texture.Apply();

        srdColor = initColor;
    }

    public void ClearColor()
    {
        InitCanvasColor();

        SetLine();
    }

    public void WidthSliderValueChangedEvent(float value)
    {
        width = (int)value;
    }

    private void SetLine()
    {
        ColourBetween(Vector2.zero, new Vector2(srd.sprite.rect.width, srd.sprite.rect.height), lineColor);
        ColourBetween(new Vector2(srd.sprite.rect.width, 0), new Vector2(0, srd.sprite.rect.height), lineColor);
        ColourBetween(new Vector2((srd.sprite.rect.width / 2), 0), new Vector2((srd.sprite.rect.width / 2), srd.sprite.rect.height), lineColor);
        ColourBetween(new Vector2(0, srd.sprite.rect.height / 2), new Vector2(srd.sprite.rect.width, srd.sprite.rect.height / 2), lineColor);

        srd.sprite.texture.SetPixels(srdColor);
        srd.sprite.texture.Apply();
    }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButton(0))
        {
            var pixel_pos = GetConvertedPos(Input.mousePosition);

            if (pixel_pos.x < 0 || pixel_pos.y < 0 || pixel_pos.x > srd.sprite.rect.width || pixel_pos.y > srd.sprite.rect.height)
            {
                return;
            }

            if (isFirst)
            {
                previousPos = pixel_pos;
                currentPos = pixel_pos;

                if (isMirrorMode)
                {
                    var vs = GetMirrorPos(currentPos);

                    foreach (var v in vs)
                    {
                        DrawByPos(v, penColor);
                    }
                }
                else
                {
                    DrawByPos(currentPos, penColor);
                }

                srd.sprite.texture.SetPixels(srdColor);
                srd.sprite.texture.Apply();

                isFirst = false;
            }
            else
            {
                currentPos = pixel_pos;

                if (isMirrorMode)
                {
                    var pvs = GetMirrorPos(previousPos);
                    var cvs = GetMirrorPos(currentPos);

                    for (int i = 0; i < pvs.Length; i++)
                    {
                        ColourBetween(pvs[i], cvs[i], penColor);
                    }
                }
                else
                {
                    ColourBetween(previousPos, currentPos, penColor);
                }

                srd.sprite.texture.SetPixels(srdColor);
                srd.sprite.texture.Apply();
                previousPos = pixel_pos;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isFirst = true;
        }
    }

    private int GetPixelIndexByPos(int x, int y, int width)
    {
        return y * width + x;
    }

    private Vector2 GetConvertedPos(Vector2 inputPos)
    {
        Vector3 local_pos = srd.transform.InverseTransformPoint(camera.ScreenToWorldPoint(inputPos));
        float pixelWidth = srd.sprite.rect.width;
        float pixelHeight = srd.sprite.rect.height;
        float unitsToPixels = pixelWidth / srd.sprite.bounds.size.x * transform.localScale.x;

        float centered_x = local_pos.x * unitsToPixels + pixelWidth / 2;
        float centered_y = local_pos.y * unitsToPixels + pixelHeight / 2;
        Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));

        return pixel_pos;
    }

    private void DrawByPos(Vector2 pixel_pos, Color color)
    {
        for (int i = (-1 * width); i < width; i++)
        {
            if (pixel_pos.x + i > srd.sprite.rect.width || pixel_pos.x + i < 0)
            {
                continue;
            }

            for (int j = (-1 * width); j < width; j++)
            {
                if (pixel_pos.y + j > srd.sprite.rect.height || pixel_pos.y + j < 0)
                {
                    continue;
                }

                DrawMethod(new Vector2(pixel_pos.x + i, pixel_pos.y + j), color);
            }
        }
    }

    private void DrawMethod(Vector2 pixel_pos, Color color)
    {
        var index = GetPixelIndexByPos((int)pixel_pos.x, (int)pixel_pos.y, (int)srd.sprite.rect.width);

        if (index >= srdColor.Length || srdColor[index] == Color.black)
        {
            return;
        }

        srdColor[index] = color;
    }

    public void ColourBetween(Vector2 start_point, Vector2 end_point, Color color)
    {
        float distance = Vector2.Distance(start_point, end_point);

        float lerp_steps = 1 / distance;

        for (float lerp = 0; lerp <= 1; lerp += lerp_steps)
        {
            DrawByPos(Vector2.Lerp(start_point, end_point, lerp), color);
        }
    }

    private Vector2[] GetMirrorPos(Vector2 pos)
    {
        pos = pos - new Vector2(srd.sprite.rect.width / 2, srd.sprite.rect.height / 2);

        int xFlag = pos.x >= 0 ? 1 : -1;
        int yFlag = pos.y >= 0 ? 1 : -1;

        Vector2 v1 = new Vector2(xFlag * Mathf.Abs(pos.y), yFlag * Mathf.Abs(pos.x));
        Vector2 v2 = new Vector2(pos.x, -pos.y);
        Vector2 v3 = new Vector2(v1.x, -v1.y);

        Vector2 v4 = new Vector2(-pos.x, pos.y);
        Vector2 v5 = new Vector2(-v1.x, v1.y);
        Vector2 v6 = new Vector2(-v2.x, v2.y);
        Vector2 v7 = new Vector2(-v3.x, v3.y);

        Vector2[] result = new Vector2[8] { pos, v1, v2, v3, v4, v5, v6, v7 };

        for (int i = 0; i < result.Length; i++)
        {
            result[i] += new Vector2(srd.sprite.rect.width / 2, srd.sprite.rect.height / 2);
        }

        return result;
    }
}
