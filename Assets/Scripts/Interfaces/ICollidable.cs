using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollidable
{
	bool Collide(PlayerEntity player_);
}
