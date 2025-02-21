import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { setSnackbar } from "../state/actions/snackbar";
import Loading from "../assets/misc/loading.jpg";
import { backend } from "../config";
import { logOut } from "../helpers";
import { useHistory } from "react-router-dom/cjs/react-router-dom.min";

const FollowButton = ({ target }) =>
{
	const dispatch = useDispatch();
	const history = useHistory();
	const currentUser = useSelector(state => state.currentUser);
	const [isLoading, setIsLoading] = useState(null);
	const [isFollowing, setIsFollowing] = useState(false);
	useEffect(() =>
	{
		if (currentUser) {
			const checkIfFollowing = async () =>
			{
				setIsLoading(true);
				const result = await fetch(`${backend}/user/following-check/${target.username}`, {
					headers: {
						Authorization: `Bearer ${localStorage.token}`
					}
				});
				if (result.status === 401)
					return logOut();
				if (!result.ok)
					return setIsFollowing(false);


				const resultJSON = await result.json();
				setIsFollowing(resultJSON);
				setIsLoading(false);
			};
			checkIfFollowing();
		}
	}, [currentUser, target]);

	const follow = async () => {
		setIsLoading(true);
		try {
			const result = await fetch(`${backend}/user/follow/${target.username}`, {
				method: "POST",
				headers: {
					Authorization: `Bearer ${localStorage.token}`
				}
			});
			if (result.status === 401)
				return logOut(dispatch, history);
			
			if (!result.ok)
			{
				const resultJSON = await result.json();
				throw new Error(resultJSON.detail, { cause: resultJSON.errors });
			}
			setIsFollowing(true);
		}
		catch (err) {
			const errors = err.cause ?? [];
			if (errors.some(e => e.code === "NotFound"))
				dispatch(setSnackbar("User not found.", "error"));
			else if (errors.some(e => e.code === "Duplicate"))
				dispatch(setSnackbar("User already followed", "error"));
			else
				dispatch(setSnackbar(err.message, "error"));
		}
		setIsLoading(false);
	};
	const unfollow = async () => {
		setIsLoading(true);
		try {
			const result = await fetch(`${backend}/user/unfollow/${target.username}`, {
				method: "POST",
				headers: {
					Authorization: `Bearer ${localStorage.token}`
				}
			});
			if (result.status === 401)
				return logOut(dispatch, history);
			
			if (!result.ok)
			{
				const resultJSON = await result.json();
				throw new Error(resultJSON.detail, { cause: resultJSON.errors });
			}
			setIsFollowing(false);
		}
		catch (err) {
			const errors = err.cause ?? [];
			if (errors.some(e => e.code === "NotFound"))
				dispatch(setSnackbar("User not found.", "error"));
			else if (errors.some(e => e.code === "Duplicate"))
				dispatch(setSnackbar("User already not followed", "error"));
			else
				dispatch(setSnackbar(err.message, "error"));
		}
		setIsLoading(false);
	};

	if (isLoading) return <button className="follow-btn loading"><div><img src={Loading} alt="loading"></img></div></button>;
	if (isFollowing)
		return <button className="follow-btn unfollow" onClick={unfollow}>Unfollow</button>;
	else
		return <button className="follow-btn follow" onClick={follow}>Follow</button>;
};

export default FollowButton;
