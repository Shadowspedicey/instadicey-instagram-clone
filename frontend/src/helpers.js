import { backend } from "./config";

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