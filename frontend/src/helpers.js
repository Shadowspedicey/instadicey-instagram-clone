import { backend } from "./config";
import { setUser } from "./state/actions/currentUser";
import { startLoading, stopLoading } from "./state/actions/isLoading";

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