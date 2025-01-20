using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;

public class PhoneRepresentation : NetworkBehaviour
{
    public GameObject hitObjectPrefab;
    public Color interactionColor {get; set;}
    public GameObject selectedObject = null;
    [Networked, OnChangedRender(nameof(SelectedObjectChanged))]
    public MovableObject networkedSelectedObject {get; set;}
    private SubplaneConfig subplaneConfig;

    public override void FixedUpdateNetwork()
    {
        if(!subplaneConfig)
            subplaneConfig = FindFirstObjectByType<SubplaneConfig>();
        if(subplaneConfig.isMirror)
            return;

        if (HasStateAuthority && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                HandleSingleTouch(touch);
            }
            /*if(touch.phase == TouchPhase.Moved){
                HandleTranslateTouch(touch);
            }*/
        }
    }

    private void HandleSingleTouch(Touch touch){
        if(!subplaneConfig)
            subplaneConfig = FindFirstObjectByType<SubplaneConfig>();
        
        if(subplaneConfig.IsConfig())
            return;

        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.red, 5f);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 direction = ray.direction;
            Debug.Log("BCZ colpisco, " + hit.transform.gameObject.name);
            Subplane hittedSubplane = hit.transform.gameObject.GetComponent<Subplane>();
            if(hittedSubplane){
                Debug.Log("Hit display global: " + hit.point);
                Debug.Log("Hit display local: " + hit.transform.InverseTransformPoint(hit.point));
                Vector3 cameraMappedPoint = hittedSubplane.NormalizedHitPoint(hit.transform.InverseTransformPoint(hit.point));
                /*if(aRSphereController){
                    Debug.Log("BCZ ID: " + aRSphereController.Id);
                    aRSphereController.SendPointToDesktop(cameraMappedPoint, direction);
                }*/
                // TODO sendremotepointrpc
                
                RaycastFromVRCameraRPC(cameraMappedPoint, direction, subplaneConfig.isMirror);
            }
            else{
                //aRSphereController.MoveSelectedObject(hit.point);
                // TODO local moving object
                SendLocalPoint(hit, direction);
                Debug.Log("Cliccato fuori");
            }
        }
    }
    private void HandleTranslateTouch(Touch touch){
        if(!subplaneConfig)
            subplaneConfig = FindFirstObjectByType<SubplaneConfig>();
        
        if(subplaneConfig.IsConfig())
            return;

        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.red, 5f);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 direction = ray.direction;
            Debug.Log("BCZ colpisco, " + hit.transform.gameObject.name);
            Subplane hittedSubplane = hit.transform.gameObject.GetComponent<Subplane>();
            if(hittedSubplane){
                Debug.Log("Hit display global: " + hit.point);
                Debug.Log("Hit display local: " + hit.transform.InverseTransformPoint(hit.point));
                Vector3 cameraMappedPoint = hittedSubplane.NormalizedHitPoint(hit.transform.InverseTransformPoint(hit.point));
                /*if(aRSphereController){
                    Debug.Log("BCZ ID: " + aRSphereController.Id);
                    aRSphereController.SendPointToDesktop(cameraMappedPoint, direction);
                }*/
                // TODO sendremotepointrpc
                
                RaycastFromVRCameraRPC(cameraMappedPoint, direction, subplaneConfig.isMirror);
            }
            else{
                //aRSphereController.MoveSelectedObject(hit.point);
                // TODO local moving object
                SendLocalPoint(hit, direction);
                Debug.Log("Cliccato fuori");
            }
        }
    }

    // Funzione che viene eseguita solo sul client nel mondo VR, quindi l'oggetto PhoneRepresentation non ha StateAuthority, motivo per cui è necessaria l'RPC e non è possibile modificare la proprietà networked selectedObject
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public async void RaycastFromVRCameraRPC(Vector3 cameraMappedPoint, Vector3 direction, bool isMirror){
        if(!PlatformManager.IsDesktop())
            return;
        
        //Vector3 pointFromClipPlane = new Vector3(cameraMappedPoint.x, cameraMappedPoint.y, Camera.main.nearClipPlane);
        Ray ray = Camera.main.ViewportPointToRay(cameraMappedPoint);
        Debug.DrawLine(ray.origin, ray.direction*5000, interactionColor, 50);
        if(!isMirror)
            Debug.DrawLine(transform.position, direction, interactionColor, 50);
        if (Physics.Raycast(ray, out RaycastHit hit)){
            if(hit.collider.tag.Equals("MovableObject")){
                if(hit.collider.gameObject == selectedObject){
                    selectedObject.GetComponent<MovableObject>().ReleaseSelection();
                    selectedObject = null;
                    
                    ResetSelectedObjectRPC();
                    
                }
                else if(selectedObject != null){
                    // deselect old selectedObjecr
                    selectedObject.GetComponent<MovableObject>().ReleaseSelection();
                    if(selectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                        selectedObject.GetComponent<NetworkObject>().ReleaseStateAuthority();
                    // select new object
                    /*selectedObject = hit.collider.gameObject;
                    selectedObject.GetComponent<Outline>().enabled = true;*/
                    if(await hit.collider.gameObject.GetComponent<MovableObject>().TrySelectObject(this)){
                        selectedObject = hit.collider.gameObject;
                        //selectedObject.GetComponent<NetworkObject>().Wa();
                        Debug.Log("" + Runner.LocalPlayer + " hasSA: " + selectedObject.GetComponent<NetworkObject>().HasStateAuthority);
                        UpdateSelectedObjectRPC(selectedObject.GetComponent<NetworkObject>().Id);
                        //selectedObject.GetComponent<MovableObject>().SetControlledByARRPC(false);
                    }
                }
                else{
                    /*selectedObject = hit.collider.gameObject;
                    selectedObject.GetComponent<Outline>().enabled = true;*/
                    if(await hit.collider.gameObject.GetComponent<MovableObject>().TrySelectObject(this)){
                        selectedObject = hit.collider.gameObject;
                        Debug.Log("" + Runner.LocalPlayer + " hasSA: " + selectedObject.GetComponent<NetworkObject>().HasStateAuthority);
                        UpdateSelectedObjectRPC(selectedObject.GetComponent<NetworkObject>().Id);
                        //selectedObject.GetComponent<MovableObject>().SetControlledByARRPC(false);
                    }
                }
            }
            else{
                if(selectedObject == null)
                    //Instantiate(hitObjectPrefab, hit.point, Quaternion.identity);
                    Runner.Spawn(hitObjectPrefab, hit.point, Quaternion.identity);
                else{
                    //selectedObject.transform.position = hit.point;
                    if(!selectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                        await selectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
                    Debug.Log("" + Runner.LocalPlayer + " hasSA: " + selectedObject.GetComponent<NetworkObject>().HasStateAuthority);
                    selectedObject.GetComponent<MovableObject>().UpdateTransform(hit.point, false);
                    //selectedObject.transform.position = hit.point;
                    //selectedObject.GetComponent<MovableObject>().controlledByAR = false;
                    //selectedObject.GetComponent<MovableObject>().SetControlledByARRPC(false);
                }
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void UpdateSelectedObjectRPC(NetworkId selectedId){
        NetworkObject obj;
        if(Runner.TryFindObject(selectedId, out obj))
            networkedSelectedObject = obj.gameObject.GetComponent<MovableObject>();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void ResetSelectedObjectRPC(){
        networkedSelectedObject = null;
    }

    public void UpdatePosition(Vector3 newPosition, Quaternion newRotation, bool isMirror){
        //Debug.Log("Chiamata updateposition");
        float z = newPosition.z;
        if(isMirror){
            z = -z;
            newRotation = Quaternion.Euler(-newRotation.eulerAngles.x, -newRotation.eulerAngles.y, newRotation.eulerAngles.z);
        }
        z += Camera.main.nearClipPlane;
        transform.rotation = newRotation;
        transform.position = new Vector3(newPosition.x, newPosition.y, z);
        //GetComponent<NetworkTransform>().Teleport(transform.position, newRotation);
        //Debug.Log("newposition: " + transform.position);
        //Debug.Log("stateauthority: " + HasStateAuthority);
    }

    public async void SendLocalPoint(RaycastHit hit, Vector3 direction){
        //controllo non necessario, perchè è controllato nella networkupdate
        if(!HasStateAuthority)
            return;
        
        if(hit.collider.tag.Equals("MovableObject")){
            Debug.LogWarning("networked obj: " + networkedSelectedObject);
            if(networkedSelectedObject && hit.collider.gameObject == networkedSelectedObject.gameObject){
                networkedSelectedObject.ReleaseSelection();
                networkedSelectedObject = null;
            }
            else if(networkedSelectedObject != null){
                // deselect old selectedObjecr
                networkedSelectedObject.ReleaseSelection();
                // select new object
                /*selectedObject = hit.collider.gameObject;
                selectedObject.GetComponent<Outline>().enabled = true;*/
                if(await hit.collider.gameObject.GetComponent<MovableObject>().TrySelectObject(this)){
                    networkedSelectedObject = hit.collider.gameObject.GetComponent<MovableObject>();
                    Debug.Log("" + Runner.LocalPlayer + " hasSA: " + networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority);
                    //networkedSelectedObject.SetControlledByARRPC(true);
                }
            }
            else{
                /*selectedObject = hit.collider.gameObject;
                selectedObject.GetComponent<Outline>().enabled = true;*/
                if(await hit.collider.gameObject.GetComponent<MovableObject>().TrySelectObject(this)){
                    networkedSelectedObject = hit.collider.gameObject.GetComponent<MovableObject>();
                    Debug.Log("" + Runner.LocalPlayer + " hasSA: " + networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority);
                    //networkedSelectedObject.SetControlledByARRPC(true);
                }
            }
        }
        else{
            if(networkedSelectedObject == null){
                NetworkObject spawned = Runner.Spawn(hitObjectPrefab, hit.point, Quaternion.identity);
                //spawned.GetComponent<MovableObject>().SetControlledByARRPC(true);
            }
            else{
                if(!networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                await networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
                Debug.Log("" + Runner.LocalPlayer + " hasSA: " + networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority);
                //networkedSelectedObject.UpdateTransformRPC(hit.point, true);
                networkedSelectedObject.transform.position = hit.point;
                networkedSelectedObject.GetComponent<MovableObject>().controlledByAR = true;
                //networkedSelectedObject.SetControlledByARRPC(true);
            }
        }        
    }

    public void SelectObject(MovableObject obj){
        networkedSelectedObject = obj;
        Debug.Log("selectedObj: " + networkedSelectedObject);
    }

    public void SelectedObjectChanged(){
        Debug.Log("Selezione cambiata: " + networkedSelectedObject + " SA: " + HasStateAuthority);
        if(networkedSelectedObject == null)
            selectedObject = null;
        else
            selectedObject = networkedSelectedObject.gameObject;
    }
}
