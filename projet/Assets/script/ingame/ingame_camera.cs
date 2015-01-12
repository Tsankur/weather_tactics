using UnityEngine;
using System.Collections;

public class ingame_camera : MonoBehaviour
{
    private Vector2 m_vMaxPosition = new Vector2(0, 0);
    public Vector2 m_vRealMaxPosition = new Vector2(0, 0);
    public Vector2 m_vRealMinPosition = new Vector2(0, 0);
    private bool m_bMoving = false;
    private Vector3 m_vLastMousePosition;
    private float m_fZoom = 150;
    // Use this for initialization
    void Start()
    {
        m_fZoom = 50;
        transform.position = new Vector3(transform.position.x, transform.position.y, -m_fZoom);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            m_vLastMousePosition = Input.mousePosition;
            m_bMoving = true;
        }
        if (m_bMoving)
        {
            transform.position += (m_vLastMousePosition - Input.mousePosition) / (800.0f / m_fZoom);
            ClampPosition();
            m_vLastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
        {
            m_bMoving = false;
        }
    }
    void ClampPosition()
    {
        float newX = transform.position.x, newY = transform.position.y;
        if (transform.position.x > m_vRealMaxPosition.x)
        {
            newX = m_vRealMaxPosition.x;
        }
        if (transform.position.y > m_vRealMaxPosition.y)
        {
            newY = m_vRealMaxPosition.y;
        }
        if (transform.position.x < m_vRealMinPosition.x)
        {
            newX = m_vRealMinPosition.x;
        }
        if (transform.position.y < m_vRealMinPosition.y)
        {
            newY = m_vRealMinPosition.y;
        }
        transform.position = new Vector3(newX, newY, -m_fZoom);
    }
    void ComputeRealMaxPositions()
    {
        float fVisionHeight = m_fZoom * Mathf.Sin(Mathf.PI / 6.0f) / Mathf.Cos(Mathf.PI / 6.0f);
        float fVisionWidth = fVisionHeight * camera.aspect;
        Vector2 vMinGamezone = new Vector2(10.0f / (float)Screen.width * fVisionWidth * 2, 10.0f / (float)Screen.width * fVisionHeight * 2);
        Vector2 vMaxGamezone = new Vector2(10.0f / (float)Screen.width * fVisionWidth * 2, 10.0f / (float)Screen.width * fVisionHeight * 2);
        m_vRealMaxPosition = new Vector2(Mathf.Max((m_vMaxPosition.x - vMinGamezone.x + vMaxGamezone.x) / 2, m_vMaxPosition.x - fVisionWidth + 5 + vMaxGamezone.x), Mathf.Max((m_vMaxPosition.y - vMinGamezone.y + vMaxGamezone.y) / 2, m_vMaxPosition.y - fVisionHeight + 5 + vMaxGamezone.y));
        m_vRealMinPosition = new Vector2(Mathf.Min((m_vMaxPosition.x - vMinGamezone.x + vMaxGamezone.x) / 2, fVisionWidth - 5 - vMinGamezone.x), Mathf.Min((m_vMaxPosition.y - vMinGamezone.y + vMaxGamezone.y) / 2, fVisionHeight - 5 - vMinGamezone.y));
    }
    public void SetMaxPosition(Vector2 _vMaxPosition)
    {
        m_vMaxPosition = _vMaxPosition;
        ComputeRealMaxPositions();
        ClampPosition();
    }
}
