using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IRenderableLayer
{
	void Update(int depth);
	void PostUpdate();
}
