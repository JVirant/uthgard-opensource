using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMapEdit.Tools
{
	public class Camera
	{
		public readonly Vector3 Up;

		public Vector3 Position;
		public Vector3 Target;

		public Camera(Vector3 up)
		{
			Up = up;
		}

	}
}
