using System;
using Unity.Behavior;

[BlackboardEnum]
public enum BossState
{
    Idle,
	Move,
	Attack,
	Guard,
	Hurt,
	Groggy,
	Dead
}
