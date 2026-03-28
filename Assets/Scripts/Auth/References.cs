using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;

public class References : MonoBehaviour
{
    public static string userName = "";
    public static string userEmail = "";
    public static int userCoins = 0;
    public static int userScore = 0;
    public static int userHealth = 0;
    public static int userLevel = 0;

    // Add Firebase references
    public static FirebaseAuth Auth => FirebaseAuthManager.Instance?.auth;
    public static FirebaseUser User => FirebaseAuthManager.Instance?.user;
    public static FirebaseFirestore Firestore => FirebaseAuthManager.Instance?.db;
    public DependencyStatus Status => FirebaseAuthManager.Instance.dependencyStatus;
}