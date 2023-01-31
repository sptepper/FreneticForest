using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

internal class CritterMoveCommand : CritterCommand
{
    public const float DELTA_FROM_TARGET = 0.02f;
    private readonly Vector3 _destination;
    private readonly NavMeshAgent _agent;

    public CritterMoveCommand(Vector3 destination, NavMeshAgent _agent)
    {
        _destination = destination;
        this._agent = _agent;
    }

    public override bool IsFinished => _agent.remainingDistance <= DELTA_FROM_TARGET;

    public override void Execute()
    {
        //Debug.Log(string.Format("Executing Move to : {0}", _destination));
        _agent.SetDestination(_destination);
    }
}
