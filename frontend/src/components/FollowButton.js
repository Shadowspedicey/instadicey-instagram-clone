import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { setSnackbar } from "../state/actions/snackbar";
import Loading from "../assets/misc/loading.jpg";

const FollowButton = ({ target }) =>
{
	const dispatch = useDispatch();
	const currentUser = useSelector(state => state.currentUser);
	const [isLoading, setIsLoading] = useState(null);
	const [isFollowing, setIsFollowing] = useState(false);
	useEffect(() =>
	{
		const checkIfFollowing = async () =>
		{
			if (!currentUser) return;
			setIsLoading(true);
			// TODO: Check if current user is following target user from DB, and set it (setIsFollowing)
			setIsLoading(false);
		};
		checkIfFollowing();
	// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [currentUser, target]);

	const follow = async () =>
	{
		if (!currentUser) return alert("sign in");
		window.onbeforeunload = () => "";
		setIsLoading(true);
		// TODO: follow the target user
		setIsFollowing(true);
		setIsLoading(false);
		window.onbeforeunload = null;
	};
	const unfollow = async () =>
	{
		window.onbeforeunload = () => "";
		setIsLoading(true);
		// TODO: unfollow the target user
		setIsFollowing(false);
		setIsLoading(false);
		window.onbeforeunload = null;
	};

	if (isLoading) return <button className="follow-btn loading"><div><img src={Loading} alt="loading"></img></div></button>;
	if (isFollowing)
		return <button className="follow-btn unfollow" onClick={unfollow}>Unfollow</button>;
	else
		return <button className="follow-btn follow" onClick={follow}>Follow</button>;
};

export default FollowButton;
