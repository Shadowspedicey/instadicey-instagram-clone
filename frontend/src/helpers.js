import { backend } from "./config";
import { setUser } from "./state/actions/currentUser";
import { startLoading, stopLoading } from "./state/actions/isLoading";
import jwt from "jsonwebtoken";

export const ping = async () =>
{
	try
	{
		const pingResult = await fetch(`${backend}/ping`, {method: "GET", mode: "cors"});
		console.log(pingResult);
		return pingResult.ok;
	}
	catch
	{
		return false;
	}
};

export const logOut = async (dispatch, history) => {
	dispatch(startLoading());
	localStorage.removeItem("token");
	dispatch(setUser(null));
	history.push("/");
	dispatch(stopLoading());
};

export const refreshOrLogout = async (dispatch, history) =>
{
	const result = await fetch(`${backend}/auth/login/refresh?rt=${encodeURIComponent(localStorage.refreshToken)}`, {
		method: "POST"
	});
	if (!result.ok)
		return logOut(dispatch, history);
	const resultJSON = await result.json();
	localStorage.token = resultJSON.token;
	localStorage.refreshToken = resultJSON.refreshToken;
	const claims = jwt.decode(resultJSON.token);
	dispatch(setUser(claims));
};