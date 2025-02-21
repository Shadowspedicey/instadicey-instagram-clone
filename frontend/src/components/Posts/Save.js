import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { setSnackbar } from "../../state/actions/snackbar";
import { backend } from "../../config";
import { logOut } from "../../helpers";
import { useHistory } from "react-router-dom/cjs/react-router-dom.min";

const Save = ({size = 25, target}) =>
{
	const dispatch = useDispatch();
	const history = useHistory();
	const currentUser = useSelector(state => state.currentUser);
	const [isSaved, setIsSaved] = useState(false);
	useEffect(() =>
	{
		const checkIfSaved = async () =>
		{
			if (!currentUser) return;

			const savedPosts = await fetch(`${backend}/user/saved-posts`, {
				headers: {
					Authorization: `Bearer ${localStorage.token}`
				}
			});
			const savedPostsJSON = await savedPosts.json();

			if (savedPosts.ok && savedPostsJSON.some(p => p.id === target.id))
				setIsSaved(true);
		};
		checkIfSaved();
	}, [currentUser, target]);

	const save = async () =>
	{
		try
		{
			const result = await fetch(`${backend}/user/saved-posts/${target.id}`, {
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
			setIsSaved(true);
		} catch (err)
		{
			dispatch(setSnackbar(err.message ?? "Oops, try again later.", "error"));
		}
	};

	const unsave = async () =>
	{
		try
		{
			const result = await fetch(`${backend}/user/saved-posts/remove/${target.id}`, {
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
			setIsSaved(false);
		} catch (err)
		{
			dispatch(setSnackbar(err.message ?? "Oops, try again later.", "error"));
		}
	};

	 if (isSaved) return(
		<button className="save saved icon" onClick={unsave}>
			<svg xmlns="http://www.w3.org/2000/svg" width={size} height={size} viewBox="0 0 24 24"><path d="M18 24l-6-5.269-6 5.269v-24h12v24z"/></svg>
		</button>
	);
	else return(
		<button className="save icon" onClick={save}>
			<svg width={size} height={size} viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" fillRule="evenodd" clipRule="evenodd"><path d="M5 0v24l7-6 7 6v-24h-14zm1 1h12v20.827l-6-5.144-6 5.144v-20.827z"></path></svg>
		</button>
	);
};

export default Save;
