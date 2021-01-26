﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
//using Vectrosity;

public class SaveManager : MonoBehaviour
{

	public ARRaycastManager arRaycaster;

	public static int MAX_LEN = 1000;       // Maximum length of the pathArray

	public GameObject arrowGameObject;      // Original instance of the arrow figure

	private Vector3[] hitArr;               // store the points of the current path
	private Vector3[] pathArray;           // store source to destination path of the location

	private GameObject[] gameArray;         // store the drawable figure of the arrow on the flat plane

	private int i;                          // Number of vector points in the current pathArray
	private int len, k;                     // totalLength of the pathArray to be iterated or stored in location X

	private int pathLen;					// length of the path of location A and B, respectively

	static float speed;                     // manage the speed of the helper rabit

	public GameObject followPrefab;         // the original figure of the rabit
	private GameObject travellingSalesman;  // replicate the figure of the rabit for the application

	private int followPathBool;             // whether the path is to followed or not

	public static bool rabitVisible;        // whether the rabit is visible on the flat plane or not

	public InputField speedText;			// Inputfields.
	public InputField startText;
	public InputField finishText;

	public Button postButton;

	private bool isUpload;

	void Start()
	{
		initialize();

		pathArray = new Vector3[MAX_LEN];       // create an array for the path1Array

		gameArray = new GameObject[MAX_LEN];    // create an array of the arrow figure

		pathLen = 0;							// initialize the pathLen to 0

		rabitVisible = false;                   // initial not visible state

		speed = 0.5f;                           // set the speed of the helper rabit

		// canvas setting
		speedText.characterLimit = 3;
		startText.characterLimit = 10;
		finishText.characterLimit = 10;

		startText.gameObject.SetActive(false);
		finishText.gameObject.SetActive(false);
		postButton.gameObject.SetActive(false);

		isUpload = false;

	}

	public void initialize()
	{
		hitArr = new Vector3[MAX_LEN];  // reset the current pathArray for the current path
		i = 0;                          // set pathArray length to 0
		len = -1;                       // set final pathArray length to -1 (for safety purposes)
		followPathBool = 0;             // set followPath Boolean to false, initially
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0) && followPathBool == 0)
		{
			List<ARRaycastHit> hits = new List<ARRaycastHit>();
			if (arRaycaster.Raycast(Input.mousePosition, hits, TrackableType.Planes))
			{
				Pose hitPose = hits[0].pose;
				hitArr[i] = hitPose.position;
				var go = GameObject.Instantiate(arrowGameObject, hitPose.position, Quaternion.identity);
				gameArray[i] = go;
				i++;
			}
		}

		if (followPathBool == 1)
		{
			float step = speed * Time.deltaTime;

			if (k < len)
			{
				if (0.02 >= Vector3.Distance(travellingSalesman.transform.position, hitArr[k]))
                {
					travellingSalesman.transform.position = hitArr[k];
				}
                else
                {
					travellingSalesman.transform.position = Vector3.MoveTowards(travellingSalesman.transform.position, hitArr[k], step);
				}

				if (travellingSalesman.transform.position == hitArr[k])
				{
					Vector3 relativePos = hitArr[k+1] - travellingSalesman.transform.position;
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
		travellingSalesman = GameObject.Instantiate(followPrefab, hitArr[0], Quaternion.identity);

		if (hitArr[1] != null)
        {
			Vector3 relativePos = hitArr[1] - travellingSalesman.transform.position;
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
			//go.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();

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

	public void savePath()
	{
		if (rabitVisible) return; // if rabit moving

		rabitVisible = true;
		pathLen = i; // n
		for (int x = 0; x < pathLen; x++)
		{
			pathArray[x] = hitArr[x];
		}
		reset();
		initialize();
	}

	public void loadPath()
	{

		if (rabitVisible == true)
		{
			Destroy(travellingSalesman); // current rabit Destroy
		}
		rabitVisible = true;
		followPathBool = 1;

		for (int x = 0; x < pathLen; x++)
		{
			hitArr[x] = pathArray[x];
		}
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

	public void upload()
    {
		if (isUpload)
        {
			isUpload = false;
			startText.gameObject.SetActive(false);
			finishText.gameObject.SetActive(false);
			postButton.gameObject.SetActive(false); 
		}
        else
        {
			isUpload = true;
			startText.gameObject.SetActive(true);
			finishText.gameObject.SetActive(true);
			postButton.gameObject.SetActive(true);
		}
	}

	public void post()
    {
		if (startText.text == "" || finishText.text == "")
        {
			isUpload = false;
			startText.gameObject.SetActive(false);
			finishText.gameObject.SetActive(false);
			postButton.gameObject.SetActive(false);
		}
        else
        {
			// startText, finishText, pathArray post.
        }
    }
}
