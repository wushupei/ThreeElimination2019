using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CellController : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler
{
    private CellController[,] cells; //二维数组保存所有格子
    private int y, x; //自身在二维数组中的索引   
    private Animator anim;
    private Image imageColor;
    private Animator effect;
    [HideInInspector] public int imageType;
    [HideInInspector] public bool homing; //图片是否归位置
    [HideInInspector] public bool deleted; //图片是否删除
    public void InitCell(CellController[,] _cells, int _y, int _x, int _imageType, Transform effects, float width) //初始化格子 
    {
        cells = _cells;
        y = _y;
        x = _x;
        anim = GetComponent<Animator>();
        imageColor = GetComponent<Image>();
        imageType = _imageType;

        imageColor.color = imageType != 0 ? Color.white : Color.blue;
        homing = true;
        deleted = true;
        //加载特效并根据格子大小设置特效图片的尺寸
        effect = Resources.Load<Animator>("Effects/Effect");
        RectTransform rect = effect.GetComponent<RectTransform>();
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width + 20);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, width + 20);
        //生成特效,需要时再播放动画
        effect = Instantiate(effect, transform.position, Quaternion.identity, effects);
    }
    bool swap; //交换
    Vector2 pointPos; //点击格子时的坐标
    public void OnPointerDown(PointerEventData eventData) //鼠标点击
    {
        if (MainGame.Instance.drap && MainGame.Instance.life > 0)
        {
            swap = true;
            pointPos = eventData.position;
        }
    }
    public void OnPointerUp(PointerEventData eventData) //鼠标弹起
    {
        swap = false;
    }
    Vector3 rayDir; //射线方向

    public void OnPointerExit(PointerEventData eventData) //鼠标移出
    {
        if (swap)
        {
            swap = false;
            Transform other = null; //交换图片的格子            
            Vector2 drapDir = eventData.position - pointPos; //移出格子时的向量      
            //判断上下左右
            if (Mathf.Abs(drapDir.x) > Mathf.Abs(drapDir.y)) //水平方向拖动
            {
                if (drapDir.x < 0) //向左拖且左面有格子
                {
                    if (x > 0)
                        other = cells[y, x - 1].transform;
                }
                else //向右拖且右面有格子
                {
                    if (x < cells.GetLength(1) - 1)
                        other = cells[y, x + 1].transform;
                }
            }
            else //垂直方向拖动
            {
                if (drapDir.y > 0) //向上拖且上面有格子
                {
                    if (y > 0)
                        other = cells[y - 1, x].transform;
                }
                else //向下拖且下面有格子
                {
                    if (y < cells.GetLength(0) - 1)
                        other = cells[y + 1, x].transform;
                }
            }
            MainGame.Instance.doubleHit = 0; //重新拖动刷新连击数
            MainGame.Instance.subLife(); //拖一次减少一次生命
            SwapImage(other); //交换格子
        }
    }
    private void SwapImage(Transform other) //交换两个格子的图片
    {
        //两张图片交换父物体
        Transform selfImage = transform.GetChild(0);
        Transform otherImage = other.GetChild(0);
        selfImage.SetParent(other.transform);
        otherImage.SetParent(transform);

        //两个格子的脚本交换图片类型
        CellController otherCell = other.GetComponent<CellController>();
        int item = otherCell.imageType;
        otherCell.imageType = imageType;
        imageType = item;
    }
    public void CheckIdentical(bool h) //检测相同图片
    {
        int length = cells.GetLength(h ? 1 : 0);
        int dir = h ? x : y;
        //检测右边或下面相同的图片
        if (dir < length - 2)
        {
            if ((h ? cells[y, x + 1] : cells[y + 1, x]).imageType == imageType && (h ? cells[y, x + 2] : cells[y + 2, x]).imageType == imageType)
            {
                MainGame.Instance.AddCell(this);
                MainGame.Instance.AddCell(h ? cells[y, x + 1] : cells[y + 1, x]);
                MainGame.Instance.AddCell(h ? cells[y, x + 2] : cells[y + 2, x]);
            }
        }
        //在不靠边时检测左右或上下相同的图片
        if (dir > 0 && dir < length - 1)
        {
            if ((h ? cells[y, x - 1] : cells[y - 1, x]).imageType == imageType && (h ? cells[y, x + 1] : cells[y + 1, x]).imageType == imageType)
            {
                MainGame.Instance.AddCell(h ? cells[y, x - 1] : cells[y - 1, x]);
                MainGame.Instance.AddCell(this);
                MainGame.Instance.AddCell(h ? cells[y, x + 1] : cells[y + 1, x]);
                //检测左边或上边是否还有相同的图片
                if (dir > 1 && (h ? cells[y, x - 2] : cells[y - 2, x]).imageType == imageType)
                    MainGame.Instance.AddCell(h ? cells[y, x - 2] : cells[y - 2, x]);
                //检测右边或下边是否还有相同的图片
                if (dir < length - 2 && (h ? cells[y, x + 2] : cells[y + 2, x]).imageType == imageType)
                    MainGame.Instance.AddCell(h ? cells[y, x + 2] : cells[y + 2, x]);
            }
        }
        //检测左边或上边相同的图片
        if (dir > 1)
        {
            if ((h ? cells[y, x - 1] : cells[y - 1, x]).imageType == imageType && (h ? cells[y, x - 2] : cells[y - 2, x]).imageType == imageType)
            {
                MainGame.Instance.AddCell(this);
                MainGame.Instance.AddCell(h ? cells[y, x - 1] : cells[y - 1, x]);
                MainGame.Instance.AddCell(h ? cells[y, x - 2] : cells[y - 2, x]);
            }
        }
    }
    public void GetImageFromAbove() //从上面获取图片
    {
        MainGame mg = MainGame.Instance;
        //往上找寻图片
        for (int i = 0; i < y; i++)
        {
            CellController upper = cells[y - 1 - i, x];
            if (upper.transform.childCount > 0)
            {
                upper.transform.GetChild(0).SetParent(transform);
                imageType = upper.imageType;
                return;
            }
        }
        //以上都没有图片则重新创建
        imageType = Random.Range(0, 20) < 1 ? 0 : Random.Range(1, mg.imageAssets.Length);
        Vector3 createPos; //生成图片位置
        //如果不在最下面
        if (y < cells.GetLength(1) - 1)
        {
            Transform image = cells[y + 1, x].transform.GetChild(0); //得到下面格子的图片
            //如果下面的图片在格子中,则生成位置在第一行格子上方,否则生成在下面的图片上方
            if (image.localPosition == Vector3.zero)
                createPos = cells[0, x].transform.position + Vector3.up * mg.cellsdis;
            else
                createPos = image.position + Vector3.up * mg.cellsdis;
        }
        //如果在最下面,则生成位置在第一行格子上方
        else
            createPos = cells[0, x].transform.position + Vector3.up * mg.cellsdis;
        //生成图片
        Instantiate(mg.imageAssets[imageType], createPos, Quaternion.identity, transform);
    }
    public void PlayDeleteAnima() //播放删除动画
    {
        anim.Play("Cell");
        deleted = false; //未删除完成
        MainGame.Instance.drap = false; //删除时不可拖动
    }
    public void DeleteImage() //删除图片(动画事件)
    {
        if (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
            effect.Play("Effect");
            deleted = true; //删除完毕
            MainGame.Instance.CheakCellDeleted();
        }
    }
    public void CheckPropCell(CellController cell) //检查水平和垂直方向所有道具图片
    {
        for (int i = 0; i < cell.x; i++) //往左边找
        {
            MainGame.Instance.AddCell(cell.cells[cell.y, cell.x - 1 - i]);
        }
        for (int i = 0; i < cell.cells.GetLength(1) - 1 - cell.x; i++) //往右边找
        {
            MainGame.Instance.AddCell(cell.cells[cell.y, cell.x + 1 + i]);
        }
        for (int i = 0; i < cell.y; i++) //往上边找
        {
            MainGame.Instance.AddCell(cell.cells[cell.y - 1 - i, cell.x]);
        }
        for (int i = 0; i < cell.cells.GetLength(0) - 1 - cell.y; i++) //往下边找
        {
            MainGame.Instance.AddCell(cell.cells[cell.y + 1 + i, cell.x]);
        }
    }
    private void Update()
    {
        //如果有子物体且子物体没在正中,则让向子物体向正中移动
        if (transform.childCount > 0)
        {
            Transform image = transform.GetChild(0);
            if (image.localPosition != Vector3.zero)
            {
                homing = false;
                MainGame.Instance.drap = false; //移动过程中不能拖动
                image.localPosition = Vector3.MoveTowards(image.localPosition, Vector3.zero, Time.deltaTime * 1000);
                if (image.localPosition.sqrMagnitude < 10)
                {
                    image.localPosition = Vector3.zero;
                    homing = true; //图片归位
                    imageColor.color = imageType != 0 ? Color.white : Color.blue;
                    MainGame.Instance.CheakCelloming();
                }
            }
        }
    }
}
