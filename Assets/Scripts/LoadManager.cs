using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
//using Vectrosity;

public class LoadManager : MonoBehaviour
{

	public ARRaycastManager arRaycaster;

	public static int MAX_LEN = 1000;       // Maximum length of the pathArray

	public GameObject arrowGameObject;      // Original instance of the arrow figure

	private Vector3[] pathArray;           // store source to destination path of the location

	private GameObject[] gameArray;         // store the drawable figure of the arrow on the flat plane

	private int i;                          // Number of vector points in the current pathArray
	private int len, k;                     // totalLength of the pathArray to be iterated or stored in location X

	private int pathLen;                    // length of the path of location A and B, respectively

	static float speed;                     // manage the speed of the helper rabit

	public GameObject followPrefab;         // the original figure of the rabit
	private GameObject travellingSalesman;  // replicate the figure of the rabit for the application

	private int followPathBool;             // whether the path is to followed or not

	public static bool rabitVisible;        // whether the rabit is visible on the flat plane or not

	public InputField speedText;            // Inputfields.

	private bool isLoaded;

	void Start()
	{
		initialize();

		pathArray = new Vector3[MAX_LEN];       // create an array for the path1Array

		gameArray = new GameObject[MAX_LEN];    // create an array of the arrow figure

		pathLen = 0;                            // initialize the pathLen to 0

		rabitVisible = false;                   // initial not visible state

		speed = 0.5f;                           // set the speed of the helper rabit

		isLoaded = false;
	}

	public void initialize()
	{
		i = 0;                          // set pathArray length to 0
		len = -1;                       // set final pathArray length to -1 (for safety purposes)
		followPathBool = 0;             // set followPath Boolean to false, initially
	}

	void Update()
	{
		if (followPathBool == 1 && isLoaded)
		{
			float step = speed * Time.deltaTime;

			if (k < len)
			{
				if (0.02 >= Vector3.Distance(travellingSalesman.transform.position, pathArray[k]))
				{
					travellingSalesman.transform.position = pathArray[k];
				}
				else
				{
					travellingSalesman.transform.position = Vector3.MoveTowards(travellingSalesman.transform.position, pathArray[k], step);
				}

				if (travellingSalesman.transform.position == pathArray[k])
				{
					Vector3 relativePos = pathArray[k + 1] - travellingSalesman.transform.position;
					Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
					travellingSalesman.transform.rotation = rotation;
					Destroy(gameArray[k]);
					k++;
				}
			}

			if (k == len)
			{
				followPathBool = 0;
				Destroy(travellingSalesman); // delete the instance

				reset();

				rabitVisible = false; // reset visibility to initial state	
			}
		}
	}

	public void follow()
	{
		if (rabitVisible)
		{ // already following
			Destroy(travellingSalesman);
		}

		followPathBool = 1;
		travellingSalesman = GameObject.Instantiate(followPrefab, pathArray[0], Quaternion.identity);

		if (pathArray[1] != null)
		{
			Vector3 relativePos = pathArray[1] - travellingSalesman.transform.position;
			Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
			travellingSalesman.transform.rotation = rotation;
		}

		if (gameArray[0] != null)
		{
			Destroy(gameArray[0]);
		}

		len = i;
		k = 1;

		rabitVisible = true;
	}

	public void drawPath(Vector3[] arr, int limit)
	{

		for (int x = 0; x < limit; x++)
		{
			var go = GameObject.Instantiate(arrowGameObject, arr[x], Quaternion.identity);

			gameArray[x] = go;
		}
	}

	public void reset()
	{
		for (int x = 0; x < i; x++)
		{
			Destroy(gameArray[x]);
		}
		initialize(); // i = 0, len = 0

		if (rabitVisible)
		{ // if rabit moving
			followPathBool = 0;
			rabitVisible = false;
			Destroy(travellingSalesman);

			rabitVisible = false;
		}
	}

	public void loadPath()
	{

		if (rabitVisible == true)
		{
			Destroy(travellingSalesman); // current rabit Destroy
		}
		rabitVisible = true;
		followPathBool = 1;
		i = pathLen; // i = n

		drawPath(pathArray, pathLen); // redraw, gameArray store
	}

	public void applySpeed()
	{
		if (speedText.text != null && float.Parse(speedText.text) <= 3f)
		{
			speed = float.Parse(speedText.text);
		}
	}

	public void get()
    {
		isLoaded = true;
		// get navigating information
		pathArray[0] = new Vector3(0, 0, 0);
		pathArray[1] = new Vector3(1, 0, 0);
    }
}
