using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[ExecuteAlways]
public class RadialGridLayoutGroup : GridLayoutGroup
{
    public enum ChildAlignmentOrder
    {
        Left_Right,
        Right_Left
    }
    public enum RadialShape
    {
        Circular,
        Spiral,
        Parabolic
    }
    [Header("RADIAL LAYOUT SETTINGS")]
    [SerializeField] protected float m_RotateY = 0;
    [SerializeField] protected float m_RotateZ = 0;
    [SerializeField] protected float m_CircularOffsetX = 0;
    [SerializeField] protected float m_CircularOffsetY = 0;

    [SerializeField] protected bool m_IgnoreCellsize;
    [SerializeField] protected float m_fDistance;
    [SerializeField] protected RadialShape m_RadialShape = RadialShape.Circular;
    [SerializeField] protected ChildAlignmentOrder m_ChildAlignmentOrder = ChildAlignmentOrder.Left_Right;
    [Range(0, 100)]
    [SerializeField] protected float m_CirclelRadiusMultiplier = 1f;

    [Range(0f, 360f)]
    [SerializeField] protected float m_MinAngle, m_MaxAngle, m_StartAngle;

    private bool m_HasCachedActiveChildTrans = false;
    public  bool HasCachedActiveChildTrans => m_HasCachedActiveChildTrans;

    public bool  IgnoreCellSize => m_IgnoreCellsize;
    public float FDistance => m_fDistance;
    public float MinAngle => m_MinAngle;
    public float MaxAngle => m_MaxAngle;
    public float StartAngle => m_StartAngle;

    [SerializeField] protected int m_ElementCount = 0;
    public int ElementCount
    {
        get { return m_ElementCount; }
        set { m_ElementCount = Mathf.Max(0, value); }
    }

    private List<RectTransform> activeChildTransforms = new List<RectTransform>();

    protected override void OnEnable()
    {
        base.OnEnable();
        CacheActiveChildTransforms();
        CalculateRadial();
    }

    private void CacheActiveChildTransforms()
    {
        activeChildTransforms.Clear();

        if (transform.childCount == 0)
        {
            return;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i) as RectTransform;
            if (child && child.gameObject.activeSelf)
            {
                activeChildTransforms.Add(child);
            }
        }
        m_HasCachedActiveChildTrans = true;
    }

    public override void SetLayoutHorizontal() { }

    public override void SetLayoutVertical() { }

    public override void CalculateLayoutInputVertical()
    {
        CalculateRadial();
    }

    public override void CalculateLayoutInputHorizontal()
    {
        CalculateRadial();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        CacheActiveChildTransforms();
        CalculateRadial();
    }
#endif

    public void CalculateRadial()
    {
        if (activeChildTransforms.Count == 0)
            return;

        CacheActiveChildTransforms();

        m_ElementCount = activeChildTransforms.Count;

        try
        {
            var constrainType   = this.constraint;
            var constraintCount = constrainType == Constraint.Flexible ? m_ElementCount : this.constraintCount;
            constraintCount = Mathf.Clamp(constraintCount, 1, constraintCount);

            Vector2 cellSizeWithSpacing = m_IgnoreCellsize ? (Vector2.one * m_fDistance) : cellSize;
            Vector2 totalSize = new Vector2(
                (cellSizeWithSpacing.x * constraintCount) + (constraintCount - 1),
                (cellSizeWithSpacing.y * constraintCount) + (constraintCount - 1));

            float startAngle = m_StartAngle;
            startAngle = Mathf.Clamp(startAngle, m_MinAngle, m_MaxAngle);
            float anglePerChild;
            int rowCount    = 1;
            int columnCount = 1;
            int childIndex  = 0;

            switch (constrainType)
            {
                case Constraint.FixedColumnCount:
                    columnCount = this.constraintCount;
                    rowCount    = Mathf.CeilToInt((float)m_ElementCount / columnCount);
                    int denominator = columnCount;
                    denominator = constraintCount <= 2 || Mathf.Abs(m_MaxAngle - m_MinAngle) > 315f ?
                                 columnCount : (columnCount - 1);

                    anglePerChild = (m_MaxAngle - m_MinAngle) / denominator;
                    break;
                case Constraint.FixedRowCount:
                    rowCount    = this.constraintCount;
                    columnCount = Mathf.CeilToInt((float)m_ElementCount / rowCount);

                    int denominatorRow = Mathf.Abs(m_MaxAngle - m_MinAngle) > 315f ?
                                         columnCount : (columnCount - 1);

                    anglePerChild = (m_MaxAngle - m_MinAngle) / denominatorRow;
                    break;
                default:

                    rowCount    = 1;
                    columnCount = m_ElementCount;
                    int denominatorFlexible = columnCount;
                    if (m_RadialShape == RadialShape.Spiral)
                    {
                        denominatorFlexible = constraintCount <= 2?
                                              columnCount : (columnCount - 1);
                    }
                    else if (m_RadialShape == RadialShape.Circular)
                    {
                        denominatorFlexible = constraintCount <= 2 || Mathf.Abs(m_MaxAngle - m_MinAngle) > 315f ?
                                              columnCount : (columnCount - 1);
                    }
                    anglePerChild = (m_MaxAngle - m_MinAngle) / denominatorFlexible;
                    break;
            }

            float fAngle = startAngle;

            switch(constrainType)
            {
                case Constraint.FixedColumnCount:
                    AlignChildPositionFixedColumnCount(totalSize, startAngle, anglePerChild, rowCount, columnCount, ref fAngle, ref childIndex);
                    break;
                case Constraint.FixedRowCount:
                case Constraint.Flexible:
                    AlignChildPositionFixedRowCount(totalSize, anglePerChild, rowCount, columnCount, ref fAngle, ref childIndex);
                    break;
            }
        }
        catch{}
    }

    private void AlignChildPositionFixedRowCount(Vector2 totalSize, float anglePerChild, int rowCount, int columnCount, ref float fAngle, ref int childIndex)
    {
        for (int col = 0; col < columnCount; col++)
        {
            for (int row = 0; row < rowCount; row++)
            {
                if (childIndex >= m_ElementCount)
                    break;

                RectTransform child = activeChildTransforms[childIndex];
                if (child != null && child.gameObject.activeSelf)
                {
                    if (!m_IgnoreCellsize)
                    {
                        child.sizeDelta = new Vector2(cellSize.x, cellSize.y);
                    }

                    float totalCircumference = CalculateCircumferenceWithShape(totalSize, row, col);
                    float radius  = totalCircumference / (2 * Mathf.PI);

                    Vector3 vAxis = startAxis == Axis.Horizontal ?
                        new Vector3(Mathf.Cos((fAngle + m_RotateY) * Mathf.Deg2Rad) + m_CircularOffsetX, Mathf.Sin((fAngle + m_RotateZ) * Mathf.Deg2Rad) + m_CircularOffsetY, 0):
                        new Vector3(Mathf.Sin((fAngle + m_RotateZ) * Mathf.Deg2Rad) + m_CircularOffsetY, Mathf.Cos((fAngle + m_RotateY) * Mathf.Deg2Rad) + m_CircularOffsetX, 0);
                    Vector3 vPos = vAxis * radius;
                    child.localPosition = vPos;

                    Vector2 pivotOffset = Vector2.zero;
                    ApplyChildAlignment(ref pivotOffset);
                    child.pivot = pivotOffset;
                    child.anchorMin = child.anchorMax = pivotOffset;
                    childIndex++;
                }
            }
            fAngle += anglePerChild;
        }
    }

    private void AlignChildPositionFixedColumnCount(Vector2 totalSize, float startAngle, float anglePerChild, int rowCount, int columnCount, ref float fAngle, ref int childIndex)
    {
        int currentRow = 0;
        for (int row = 0; row < rowCount; row++)
        {
            currentRow = row;
            for (int col = 0; col < columnCount; col++)
            {
                if (childIndex >= m_ElementCount)
                    break;

                RectTransform child = activeChildTransforms[childIndex];

                if (child != null && child.gameObject.activeSelf)
                {
                    if (!m_IgnoreCellsize)
                    {
                        child.sizeDelta = new Vector2(cellSize.x, cellSize.y);
                    }

                    float totalCircumference = CalculateCircumferenceWithShape(totalSize, row, col);
                    float radius  = totalCircumference / (2 * Mathf.PI);

                    Vector3 vAxis = startAxis == Axis.Horizontal ?
                        new Vector3(Mathf.Cos((fAngle + m_RotateY) * Mathf.Deg2Rad) + m_CircularOffsetX, Mathf.Sin((fAngle + m_RotateZ) * Mathf.Deg2Rad) + m_CircularOffsetY, 0) :
                        new Vector3(Mathf.Sin((fAngle + m_RotateZ) * Mathf.Deg2Rad) + m_CircularOffsetY, Mathf.Cos((fAngle + m_RotateY) * Mathf.Deg2Rad) + m_CircularOffsetX, 0);

                    Vector3 vPos = vAxis * radius;
                    child.localPosition = vPos;

                    Vector2 pivotOffset = Vector2.zero;
                    ApplyChildAlignment(ref pivotOffset);
                    child.pivot = pivotOffset;
                    child.anchorMin = child.anchorMax = pivotOffset;

                    fAngle += anglePerChild;

                    childIndex++;
                }
            }

            fAngle = startAngle;
        }

        fAngle = startAngle;
        for (int col = 0; col < columnCount; col++)
        {
            for (int row = currentRow +1; row < rowCount; row++)
            {
                if (childIndex >= m_ElementCount)
                    break;

                RectTransform child = activeChildTransforms[childIndex];

                if (child != null && child.gameObject.activeSelf)
                {
                    if (!m_IgnoreCellsize)
                    {
                        child.sizeDelta = new Vector2(cellSize.x, cellSize.y);
                    }

                    float totalCircumference = CalculateCircumferenceWithShape(totalSize, currentRow, col);
                    float radius  = totalCircumference / (2 * Mathf.PI);

                    Vector3 vAxis = startAxis == Axis.Horizontal ?
                        new Vector3(Mathf.Cos((fAngle + m_RotateY) * Mathf.Deg2Rad) + m_CircularOffsetX, Mathf.Sin((fAngle + m_RotateZ) * Mathf.Deg2Rad) + m_CircularOffsetY, 0):
                        new Vector3(Mathf.Sin((fAngle + m_RotateZ) * Mathf.Deg2Rad) + m_CircularOffsetY, Mathf.Cos((fAngle + m_RotateY) * Mathf.Deg2Rad) + m_CircularOffsetX, 0);

                    Vector3 vPos = vAxis * radius;
                    child.localPosition = vPos;

                    Vector2 pivotOffset = Vector2.zero;
                    ApplyChildAlignment(ref pivotOffset);
                    child.pivot = pivotOffset;
                    child.anchorMin = child.anchorMax = pivotOffset;

                    childIndex++;
                }
            }
            fAngle += anglePerChild;
        }
    }
    private float CalculateCircumferenceWithShape(Vector2 totalSize, int currentRow, int currentCol)
    {
        float totalCircumference = 0;
        int direction = 1;
        switch (m_ChildAlignmentOrder)
        {
            case ChildAlignmentOrder.Left_Right:
                direction = -1;
                break;
            case ChildAlignmentOrder.Right_Left:
                direction = 1;
                break;
        }
        switch (m_RadialShape)
        {
            case RadialShape.Circular:
                Vector2 circularVec = new Vector2(
                                      (currentRow + 1) * m_CirclelRadiusMultiplier,
                                      (currentRow + 1) * m_CirclelRadiusMultiplier);
                circularVec += new Vector2(currentRow * spacing.x, currentCol * spacing.y) / 10f;
                totalCircumference = startAxis == Axis.Horizontal ?
                                     totalSize.x * circularVec.x :
                                     totalSize.y * circularVec.y;
                break;
            case RadialShape.Spiral:
                Vector2 spiralVec = new Vector2(
                                    (((currentRow + 1) + (currentCol + 1)) * Mathf.PI * Mathf.Deg2Rad) * m_CirclelRadiusMultiplier,
                                    (((currentRow + 1) + (currentCol + 1)) * Mathf.PI * Mathf.Deg2Rad) * m_CirclelRadiusMultiplier);
                spiralVec += new Vector2(currentRow * spacing.x, currentCol * spacing.y) / 10f;
                totalCircumference = startAxis == Axis.Horizontal ?
                                     totalSize.x * spiralVec.x * Mathf.PI * 2 :
                                     totalSize.y * spiralVec.y * Mathf.PI * 2;
                break;
            case RadialShape.Parabolic:
                break;
        }
        return totalCircumference * direction;
    }
    private void ApplyChildAlignment(ref Vector2 pivotOffset)
    {
        switch (childAlignment)
        {
            case TextAnchor.UpperLeft:
            case TextAnchor.MiddleLeft:
            case TextAnchor.LowerLeft:
                pivotOffset.x = 0f;
                break;
            case TextAnchor.UpperCenter:
            case TextAnchor.MiddleCenter:
            case TextAnchor.LowerCenter:
                pivotOffset.x = 0.5f;
                break;
            case TextAnchor.UpperRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.LowerRight:
                pivotOffset.x = 1f;
                break;
        }

        switch (childAlignment)
        {
            case TextAnchor.UpperLeft:
            case TextAnchor.UpperCenter:
            case TextAnchor.UpperRight:
                pivotOffset.y = 0f;
                break;
            case TextAnchor.MiddleLeft:
            case TextAnchor.MiddleCenter:
            case TextAnchor.MiddleRight:
                pivotOffset.y = 0.5f;
                break;
            case TextAnchor.LowerLeft:
            case TextAnchor.LowerCenter:
            case TextAnchor.LowerRight:
                pivotOffset.y = 1f;
                break;
        }
        pivotOffset += new Vector2(padding.left, padding.bottom) / 10f - new Vector2(padding.right, padding.top) / 10f;
    }
#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        Color GMcolor1 = Color.green;
        Gizmos.color = GMcolor1;
        Vector3 center = transform.position;

        var constrainType = this.constraint;
        var constraintCount = constrainType == Constraint.Flexible ? m_ElementCount : this.constraintCount;

        float radius = m_fDistance;

        float angleStep = constraintCount > 1 ? (m_MaxAngle - m_MinAngle) / (constraintCount - 1) : 0;

        if (angleStep == 0)
        {
            Gizmos.DrawLine(center, center);
            return;
        }

        for (float angle = m_MinAngle; angle <= m_MaxAngle; angle += angleStep)
        {
            float adjustedAngle = angle;

            float x = center.x + Mathf.Cos(adjustedAngle * Mathf.Deg2Rad) * radius;
            float y = center.y + Mathf.Sin(adjustedAngle * Mathf.Deg2Rad) * radius;
            Vector3 point = new Vector3(x, y, center.z);

            Gizmos.DrawLine(center, point);
        }

        Color GMcolor2 = Color.yellow;
        Gizmos.color = GMcolor2;

        if (angleStep == 0)
        {
            Gizmos.DrawLine(center, center);
            return;
        }

        float startAngleRad = m_StartAngle * Mathf.Deg2Rad;
        for (float angle = m_MinAngle; angle <= m_MaxAngle; angle += angleStep)
        {
            float adjustedAngle = angle + m_StartAngle;
            var startAxisPosX = startAxis == Axis.Horizontal ?
                                Mathf.Cos(adjustedAngle * Mathf.Deg2Rad) :
                                Mathf.Sin(adjustedAngle * Mathf.Deg2Rad);
            var startAxisPosY = startAxis == Axis.Horizontal ?
                                Mathf.Sin(adjustedAngle * Mathf.Deg2Rad) :
                                Mathf.Cos(adjustedAngle * Mathf.Deg2Rad);
            float x = center.x + startAxisPosX * radius;
            float y = center.y + startAxisPosY * radius;
            Vector3 point = new Vector3(x, y, center.z);
            Gizmos.DrawLine(center, point);
        }
    }
#endif
}
