import { useState } from "react";
import { Link } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import { format, formatDistanceToNowStrict } from "date-fns";
import VerifiedTick from "../VerifiedTick";
import Like from "./Like";
import { backend } from "../../config";
import { logOut } from "../../helpers";
import { useHistory } from "react-router-dom/cjs/react-router-dom.min";
import { setSnackbar } from "../../state/actions/snackbar";

const Comment = ({ commentData, postData, noPhoto, noTimestamp, noLike, noLikesCounter, noRemove, setLikesWindow, refreshComments}) =>
{
	const dispatch = useDispatch();
	const history = useHistory();

	const currentUser = useSelector(state => state.currentUser);
	const createdAtDate = new Date(commentData.createdAt);
	const [isLoading, setIsLoading] = useState(false);

	const removeComment = async () =>
	{
		try
		{
			setIsLoading(true);
			const result = await fetch(`${backend}/comment/delete/${commentData.id}`, {
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
			await refreshComments();
		} catch (err)
		{
			dispatch(setSnackbar(err.message, "error"));
		}
		setIsLoading(false);
	};

	return(
		<div className={`comment ${isLoading ? "disabled" : null}`} style={noLike ? null : {paddingRight: "25px"}}>
			{noPhoto || <Link to={`/${commentData.user.username}`}><div className="profile-pic"><img src={commentData.user.profilePic} alt={`${commentData.user.username}'s profile pic`}></img></div></Link>}
			<div className="info">
				<div style={{display: "inline-block"}}>
					<Link to={`/${commentData.user.username}`} className="username">{commentData.user.username}</Link>
					<VerifiedTick size={12.5} user={commentData.user} marginLeft={0} marginRight={7.5}/>
					<span className="text">{commentData.comment}</span>
				</div>
				<div>
					{noTimestamp || <span className="timestamp" title={format(createdAtDate, "d MMM, yyyy")}>{formatDistanceToNowStrict(createdAtDate)}</span>}
					{ noLikesCounter || commentData.likes.length === 0 ||
						<span className="likes" onClick={() => setLikesWindow(commentData.likes)}>{commentData.likes.length} likes</span>
					}
					{currentUser && (currentUser.username === commentData.user.username || currentUser.username === postData.user.username) && !noRemove && <button className="icon" style={{width: 15, height: 15}} onClick={removeComment}><svg width="15" height="15" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" fillRule="evenodd" clipRule="evenodd"><path d="M9 3h6v-1.75c0-.066-.026-.13-.073-.177-.047-.047-.111-.073-.177-.073h-5.5c-.066 0-.13.026-.177.073-.047.047-.073.111-.073.177v1.75zm11 1h-16v18c0 .552.448 1 1 1h14c.552 0 1-.448 1-1v-18zm-10 3.5c0-.276-.224-.5-.5-.5s-.5.224-.5.5v12c0 .276.224.5.5.5s.5-.224.5-.5v-12zm5 0c0-.276-.224-.5-.5-.5s-.5.224-.5.5v12c0 .276.224.5.5.5s.5-.224.5-.5v-12zm8-4.5v1h-2v18c0 1.105-.895 2-2 2h-14c-1.105 0-2-.895-2-2v-18h-2v-1h7v-2c0-.552.448-1 1-1h6c.552 0 1 .448 1 1v2h7z"/></svg></button>}
				</div>
			</div>
			{noLike || <Like size={15} target={commentData} isComment/>}
		</div>
	);
};

export default Comment;
