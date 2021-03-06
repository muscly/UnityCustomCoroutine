﻿using UnityEngine;
using System;
using System.Collections;
using AltProg;
using Object = System.Object;

namespace AltProg.Test
{

public class TestMain : MonoBehaviour 
{
	int updateCount;

	bool executedFinally;

	void Start () 
	{
		Coroutines.Start( TestYieldNull() );

		Coroutines.Start( TestAsyncApi(0) );
		Coroutines.Start( TestAsyncApi(0.1f) );

		Coroutines.Start( TestTooFastCallback() );

		Coroutines.Start( TestMultiLevel() );

		//Coroutines.Start( TestException() );

		// Unity Coroutine
		//StartCoroutine( TestException() );
	
		Coroutines.Start( TestGetResult() );
		Coroutines.Start( TestGetResult_InvalidType() );
		Coroutines.Start( TestGetResult_Exception() );

		Coroutines.Start( TestAsyncApiWrapper_Normal() );
		Coroutines.Start( TestAsyncApiWrapper_BeginException() );
		Coroutines.Start( TestAsyncApiWrapper_EndException() );

		Coroutines.Start( TestTryFinally() );
	}

	void Update()
	{
		++ updateCount;
	}

	IEnumerator TestYieldNull()
	{
		TestUtil.AssertEq( 0, updateCount );

		yield return null;

		TestUtil.AssertEq( 1, updateCount );

		yield return null;

		TestUtil.AssertEq( 2, updateCount );
	}

	IEnumerator TestAsyncApi( float delaySeconds )
	{
		IAsyncResult ar = TestApi.Begin_AddAsync( Coroutines.AsyncCallback, 3, 5, delaySeconds );
		yield return ar;

		int res = TestApi.End_AddAsync( ar );

		TestUtil.AssertEq( 8, res);
	}

	IEnumerator TestTooFastCallback()
	{
		IAsyncResult ar = TestApi.Begin_TooFastCallback( Coroutines.AsyncCallback, 10 );
		yield return ar;

		int res = TestApi.End_TooFastCallback( ar );

		TestUtil.AssertEq( 10, res);
	}

	IEnumerator TestMultiLevel()
	{
		TestUtil.AssertEq( 0, updateCount );

		yield return Coroutines.Start( Count2() );

		TestUtil.AssertEq( 3, updateCount );
	}

	IEnumerator Count2()
	{
		TestUtil.AssertEq( 0, updateCount );

		yield return null;

		TestUtil.AssertEq( 1, updateCount );

		yield return null;

		TestUtil.AssertEq( 2, updateCount );
	}

	/*
	IEnumerator TestException()
	{
		throw new Exception("Test");

		yield return null;

		TestUtil.Fail( "After Exception" );
	}
	*/

	IEnumerator TestGetResultApi()
	{
		yield return null;
		yield return Result.New(33);
		throw new Exception("Do Not Reach");
	}

	IEnumerator TestGetResult()
	{
		var co = Coroutines.Start<int>( TestGetResultApi() );
		yield return co;

		TestUtil.AssertEq( 33, co.Get() );
	}

	IEnumerator TestGetResult_InvalidType()
	{
		var co = Coroutines.Start<string>( TestGetResultApi() );
		yield return co;

		TestUtil.AssertThrow<Exception>( () => co.Get() );
	}

	IEnumerator TestGetResultApi_Exception()
	{
		yield return null;
		throw new Exception("Exception in Coroutine");
	}

	IEnumerator TestGetResult_Exception()
	{
		var co = Coroutines.Start<int>( TestGetResultApi_Exception() );
		yield return co;

		TestUtil.AssertThrow<Exception>( () => co.Get() );
	}

	IEnumerator TestAsyncApiWrapper_Normal()
	{
		var co = Coroutines.Start<int>( TestApiWrapper.AddAsync( 3, 5, 0.1f ) );
		yield return co;

		TestUtil.AssertEq( 8, co.Get() );
	}

	IEnumerator TestAsyncApiWrapper_BeginException()
	{
		// Set 0 to the first argument to generate an exception in Begin_AddAsync()
		var co = Coroutines.Start<int>( TestApiWrapper.AddAsync( 0, 5, 0.1f ) );

		yield return co;

		TestUtil.AssertThrow<Exception>( () => co.Get() );
	}

	IEnumerator TestAsyncApiWrapper_EndException()
	{
		// Set 0 to the second argument to generate an exception in End_AddAsync()
		var co = Coroutines.Start<int>( TestApiWrapper.AddAsync( 3, 0, 0.1f ) );
		yield return co;

		TestUtil.AssertThrow<Exception>( () => co.Get() );
	}

	IEnumerator UseTryFinally()
	{
		try
		{
			yield return null;

		}
		finally
		{
			executedFinally = true;
		}
	}

	IEnumerator UseTryFinally_WithException()
	{
		try
		{
			yield return null;

			if ( executedFinally == false )
				throw new Exception("UseTryFinally_WithException()");

			yield return null;
		}
		finally
		{
			executedFinally = true;
		}
	}

	IEnumerator TestTryFinally()
	{
		executedFinally = false;
		yield return Coroutines.Start( UseTryFinally() );

		TestUtil.AssertEq( true, executedFinally );



		executedFinally = false;
		yield return Coroutines.Start( UseTryFinally_WithException() );

		TestUtil.AssertEq( true, executedFinally );
	}

}

}
