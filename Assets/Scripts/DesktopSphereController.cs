using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;

public class DesktopSphereController : NetworkBehaviour
{

    public GameObject hitObjectPrefab;
    public GameObject phoneRepresentationPrefab;
    private GameObject selectedObject;
    public Dictionary<PlayerRef, GameObject> playersRepresentation {get; set;}
    private int playersConfiguring = 0;
    public GameObject ImageTrackingCanvasPrefab;
    private GameObject ImageTrackingCanvasInstance;

    void Start()
    {
        ImageTrackingCanvasInstance = Instantiate(ImageTrackingCanvasPrefab);
        playersRepresentation = new Dictionary<PlayerRef, GameObject>();
        // screen height in cm
        float screenHeight = (Screen.height/Screen.dpi)*2.54f;
        // frustum height in m
        float frustumHeight = screenHeight/100;
        float distance = frustumHeight * 0.5f / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        Camera.main.nearClipPlane = distance;
        transform.position = Camera.main.transform.position + new Vector3(0, 0, distance + 1);
        Debug.Log("BCZ frustum distance: " + distance);
    }

    void Update()
    {

    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void ToggleConfiguringRpc(bool playerIsConfiguring){
        Debug.Log("Players Configuring: " + playersConfiguring);
        if(playerIsConfiguring)
            playersConfiguring++;
        else
            playersConfiguring--;
        if(playersConfiguring > 0)
            ImageTrackingCanvasInstance.SetActive(true);
        else
            ImageTrackingCanvasInstance.SetActive(false);
    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void SendRemotePointRpc(Vector3 point, Vector3 direction, bool isMirror, PlayerRef callingPlayer, PhoneRepresentation callingPhone){
        Vector3 pointFromCamera = Camera.main.ViewportToWorldPoint(new Vector3(point.x, point.y, Camera.main.nearClipPlane));

        /*Debug.Log("BCZ ricevuto punto, x: " + point.x);
        Debug.Log("BCZ ricevuto punto, y: " + point.y);
        Vector3 invertedPoint = new Vector3(point.x, point.y, Camera.main.nearClipPlane);
        Ray ray = Camera.main.ViewportPointToRay(invertedPoint);
        Debug.DrawLine(ray.origin, ray.direction*5000, Color.red, 50);
        if(!isMirror)
            Debug.DrawLine(transform.position, direction, Color.blue, 50);
        if (Physics.Raycast(ray, out RaycastHit hit)){
            if(hit.collider.tag.Equals("MovableObject")){
                if(hit.collider.gameObject == selectedObject){
                    selectedObject.GetComponent<Outline>().enabled = false; 
                    selectedObject = null;
                }
                else if(selectedObject != null){
                    // deselect old selectedObjecr
                    selectedObject.GetComponent<Outline>().enabled = false;
                    // select new object
                    selectedObject = hit.collider.gameObject;
                    selectedObject.GetComponent<Outline>().enabled = true;
                }
                else{
                    selectedObject = hit.collider.gameObject;
                    selectedObject.GetComponent<Outline>().enabled = true;
                }
            }
            else{
                if(selectedObject == null)
                    //Instantiate(hitObjectPrefab, hit.point, Quaternion.identity);
                    Runner.Spawn(hitObjectPrefab, hit.point, Quaternion.identity);
                else
                    selectedObject.transform.position = hit.point;
            }
        }*/
        Debug.Log("BCZ ricevuto punto, x: " + point.x);
        Debug.Log("BCZ ricevuto punto, y: " + point.y);
        Vector3 invertedPoint = new Vector3(point.x, point.y, Camera.main.nearClipPlane);
        Ray ray = Camera.main.ViewportPointToRay(invertedPoint);
        /*Debug.DrawLine(ray.origin, ray.direction*5000, callingPhone.interactionColor, 50);
        if(!isMirror)
            Debug.DrawLine(transform.position, direction, callingPhone.interactionColor, 50);
        */if (Physics.Raycast(ray, out RaycastHit hit)){
            if(hit.collider.tag.Equals("MovableObject")){
                if(hit.collider.gameObject == selectedObject){
                    selectedObject.GetComponent<MovableObject>().ReleaseSelection();
                    selectedObject = null;
                }
                else if(selectedObject != null){
                    // deselect old selectedObjecr
                    selectedObject.GetComponent<MovableObject>().ReleaseSelection();
                    // select new object
                    /*selectedObject = hit.collider.gameObject;
                    selectedObject.GetComponent<Outline>().enabled = true;*/
                    if(hit.collider.gameObject.GetComponent<MovableObject>().TrySelectObject(callingPhone).Result)
                        selectedObject = hit.collider.gameObject;
                }
                else{
                    /*selectedObject = hit.collider.gameObject;
                    selectedObject.GetComponent<Outline>().enabled = true;*/
                    if(hit.collider.gameObject.GetComponent<MovableObject>().TrySelectObject(callingPhone).Result)
                        selectedObject = hit.collider.gameObject;
                }
            }
            else{
                if(selectedObject == null)
                    //Instantiate(hitObjectPrefab, hit.point, Quaternion.identity);
                    Runner.Spawn(hitObjectPrefab, hit.point, Quaternion.identity);
                else
                    selectedObject.transform.position = hit.point;
            }
        }/*
        if(playersRepresentation.ContainsKey(callingPlayer)){
            playersRepresentation[callingPlayer].GetComponent<PhoneRepresentation>().SendRemotePoint(point, direction, isMirror);
        }
        else{
            Debug.LogError("Non c'è il playerRepresentation corrispondente");
        }*/
    }

    /*[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public PhoneRepresentation GetPhoneRepresentationByPlayerRefRpc(PlayerRef currentPlayer){
        return playersRepresentation[currentPlayer].GetComponent<PhoneRepresentation>();
    }*/
}
