using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraFollow : MonoBehaviour

{
    [Header("目标")]
    public Transform target;
    [Header("偏移")]
    public Vector3 offset = new Vector3(0, 2, -5);
    [Header("旋转")]
    public float sensitivity = 0.1f;
    [Header("缩放")]
    public float zoomSpeed = 2f;
    public float minDistance = 2f;
    public float maxDistance = 10f;

    [Header("抖动")]
    public float shakeFrequency = 30f;

    private CameraInput cameraInput;
    private Vector2 lookInput;
    private float scrollInput;
    private float yaw;
    private float pitch;
    private float targetDistance;
    private float currentDistance;

    // 屏幕抖动
    private float shakeIntensity;
    private float shakeRemaining;

    public static CameraFollow Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cameraInput = new CameraInput();

        cameraInput.Enable();
        // 初始化距离
        currentDistance = offset.magnitude;
        targetDistance = currentDistance;

    }
    private void LateUpdate()
    {

        if (target == null)
            return;
        lookInput = cameraInput.CameraMgr.LookAround.ReadValue<Vector2>();
        scrollInput = cameraInput.CameraMgr.Zoom.ReadValue<float>();

        yaw += lookInput.x * sensitivity;
        pitch -= lookInput.y * sensitivity;

        pitch = Mathf.Clamp(pitch, -30f, 60f);
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        targetDistance -= scrollInput * zoomSpeed;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        currentDistance = Mathf.Lerp(
            currentDistance,
            targetDistance,
            10f * Time.deltaTime
        );

        offset = offset.normalized * currentDistance;
        Vector3 targetPos = target.position + rotation * offset;
        targetPos += GetShakeOffset();

        transform.position = targetPos;
        transform.LookAt(target.position + Vector3.up * 1.5f);

    }

    // ---- 屏幕抖动 ----
    public Vector3 GetShakeOffset()
    {
        if (shakeRemaining <= 0f)
            return Vector3.zero;

        shakeRemaining -= Time.deltaTime;
        float t = Mathf.Clamp01(shakeRemaining / 0.3f);   // 衰减
        float intensity = shakeIntensity * t;

        float x = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f * intensity;
        float y = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * 2f * intensity;

        return new Vector3(x, y, 0f);
    }

    public void Shake(float intensity, float duration)
    {
        shakeIntensity = Mathf.Max(shakeIntensity, intensity);
        shakeRemaining = Mathf.Max(shakeRemaining, duration);
    }

    public static void Trigger(float intensity, float duration)
    {
        if (Instance != null)
            Instance.Shake(intensity, duration);
    }
}
