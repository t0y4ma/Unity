using UnityEngine;

public class SplitEffect : MonoBehaviour
{
    public Vector3 target = new Vector3(-100000000, 0, 0);

    public float baseSpeed = 150f;      // 基本速度
    public float maxSpeed = 400f;
    public float rotateSpeed = 360f;   // 基本回転速度
    public float lifeTime = 0.5f;
    private float time;

    public void SetTarget(Vector3 newTarget)
    {
        target = newTarget;
        time = 0f;
        
        Vector3 d = (target - transform.position).normalized;

        float maxAngle = 80f;

        Vector3 startDir = Vector3.RotateTowards(
            d,
            Random.onUnitSphere,
            maxAngle * Mathf.Deg2Rad,
            0f
        );

        transform.rotation = Quaternion.LookRotation(startDir);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        time += dt;

        float t = time / lifeTime;

        // =========================
        // ターゲット方向
        // =========================
        Vector3 toTarget = target - transform.position;
        float dist = toTarget.magnitude;

        if (dist < 0.0001f)
        {
            OnHit();
            return;
        }

        Vector3 targetDir = toTarget / dist;

        // =========================
        // 回転（見た目用ホーミング）
        // =========================
        float dynamicRotate = rotateSpeed * (1f + 3f / (dist + 0.1f));

        Quaternion targetRot = Quaternion.LookRotation(targetDir);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            dynamicRotate * dt
        );

        // =========================
        // 速度（到達保証）
        // =========================
        float remainTime = Mathf.Max(0.01f, lifeTime - time);
        float needSpeed = dist / remainTime;

        float speedT = t * t;
        float visualSpeed = Mathf.Lerp(baseSpeed, maxSpeed, speedT);

        float speed = Mathf.Max(needSpeed, visualSpeed);

        // =========================
        // 移動（←ここが重要な修正）
        // =========================
        Vector3 moveDir = (targetDir * 0.85f + transform.forward * 0.15f).normalized;

        transform.position += moveDir * speed * dt;

        // =========================
        // 到達
        // =========================
        if (dist < 0.05f || time >= lifeTime)
        {
            transform.position = target;
            OnHit();
        }
    }

    void OnHit()
    {
        // 最後にピタッと合わせる
        transform.position = target;

        Destroy(gameObject);
    }
}
