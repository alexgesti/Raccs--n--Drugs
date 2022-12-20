using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayScript : MonoBehaviour
{
    public GameObject racoon;
    public GameObject cocaine;

    [HideInInspector]
    public List<GameObject> racoonList;
    public List<GameObject> cocaineList;

    private bool spawnCocaine = false;
    public bool cameraTransition = false;

    public int maxCocaineBags;
    public float offsetCocaineSpawn;

    [HideInInspector]
    public connection conect;
    private Transform playableArea;
    private Transform camera;
    private Transform gamePos;



    public void Reset()
    {
        if (racoonList != null)
            DeleteList();
        cocaineList = new List<GameObject>();
        cocaineList.Add(null);

        if (racoonList != null)
            racoonList.Clear();
        racoonList = new List<GameObject>();
    }

    void Awake()
    {
        Reset(); 

        playableArea = transform.GetChild(0);
        camera = GameObject.Find("Main Camera").transform;
        gamePos = GameObject.Find("CameraGamePosition").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraTransition)
        {
            camera.transform.position = Vector3.Lerp(camera.transform.position, gamePos.position, 2 * Time.deltaTime);
            camera.transform.rotation = Quaternion.Lerp(camera.transform.rotation, gamePos.rotation, 2 * Time.deltaTime);

            if (Vector3.Distance(camera.transform.position, gamePos.position) < 0.15)
            {
                cameraTransition = false;
                DeleteList();
            }
        }

        if (spawnCocaine)
            SpawnCocaine();

        if (!cameraTransition && cocaineList.Count <= 0)
            spawnCocaine = true;
    }

    private void FixedUpdate()
    {
        if(racoonList.Count > 0)
            conect.SendClientData(3);
    }



    public void LaunchGame(int size = 0)
    {
        cameraTransition = true;
        GameObject.Find("UI").SetActive(false);

        //SendStart
        conect.SendClientData(0);

        Transform[] pos = GameObject.Find("RacoonSpawn").GetComponentsInChildren<Transform>();
        for (int i = 0; i < size; i++)
        {
            if (i > 4)
                break;

            GameObject rac = Instantiate(racoon, pos[i + 1].position, pos[i + 1].rotation);
            rac.GetComponent<RacoonBehaviour>().ChangeState(1);
            racoonList.Add(rac);
        }
    }

    public void UpdateRacoon(Vector3 position, int posRacoon)
    {
        if (racoonList.Count > 0)
            racoonList[posRacoon].transform.position = position;
    }

    public void SpawnCocaine()
    {
        cocaineList.Clear();

        for (int i = 0; i < maxCocaineBags; i++)
        {
            Vector3 randPosition = new Vector3(
                playableArea.position.x + Random.Range(offsetCocaineSpawn, playableArea.GetComponent<Renderer>().bounds.size.x - offsetCocaineSpawn),
                cocaine.transform.position.y,
                playableArea.position.z - Random.Range(offsetCocaineSpawn, playableArea.GetComponent<Renderer>().bounds.size.z - offsetCocaineSpawn));

            GameObject obj = Instantiate(cocaine, randPosition, cocaine.transform.rotation);
            obj.GetComponent<CocaineBehaviour>().gameplayScript = this;
            obj.GetComponent<CocaineBehaviour>().isBuffed = i + 1 >= maxCocaineBags ? true : false;
            cocaineList.Add(obj);
        }


        spawnCocaine = false;
    }

    public void UpdateCocaine(Vector3 position, int posRacoon, bool isBuffed = false)
    {
        GameObject obj = Instantiate(cocaine, position, cocaine.transform.rotation);
        obj.GetComponent<CocaineBehaviour>().gameplayScript = this;
        obj.GetComponent<CocaineBehaviour>().isBuffed = isBuffed;
        cocaineList.Add(obj);
    }

    public void UpdateList(GameObject obj)
    { cocaineList.Remove(obj); }

    public void DeleteList()
    {
        foreach (GameObject coc in cocaineList)
            Destroy(coc);

        cocaineList.Clear();
    }
}
