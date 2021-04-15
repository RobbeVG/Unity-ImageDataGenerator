using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WanderBehaviour : MonoBehaviour
{
    public GameObject Target;

    public float WanderDistance = 50f;
    public float WanderTime = 15f;

    private NavMeshAgent _agent;
    private Vector3 _horizontalScale = new Vector3(1, 0, 1);
    private float _timer = 0f;

    private Vector3 _previousPos;
    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        SetNewTarget();
        _previousPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;

        if (_previousPos == transform.position && _timer > 2)
        {
            _previousPos = transform.position;
            SetNewTarget();
            return;
        }

        _previousPos = transform.position;

        if (_timer > WanderTime)
        {
            SetNewTarget();
            return;
        }

        Vector3 scaled_diff = Vector3.Scale(Target.transform.position - transform.position, _horizontalScale);
        float horizontal_sqr_distance = scaled_diff.sqrMagnitude;
        if (horizontal_sqr_distance < 2f)
            SetNewTarget();
    }

    private void SetNewTarget()
    {
        Vector3 rand_dir = transform.position + Random.insideUnitSphere * WanderDistance;

        NavMeshHit hit;
        if (!NavMesh.SamplePosition(rand_dir, out hit, WanderDistance, _agent.areaMask))
        {
            Debug.Log("No new target position found!");
            return;
        }

        Target.transform.position = hit.position;
        _agent.SetDestination(hit.position);

        _timer = 0f;
    }
}
