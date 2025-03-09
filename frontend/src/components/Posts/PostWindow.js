import { useRef, useState, useEffect } from "react";
import { Link, useHistory, useLocation } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import { format, formatDistanceToNowStrict, getYear } from "date-fns";
import { setSnackbar } from "../../state/actions/snackbar";
import FollowButton from "../FollowButton";
import FollowWindow from "../FollowWindow";
import Like from "./Like";
import Comment from "./Comment";
import Save from "./Save";
import VerifiedTick from "../VerifiedTick";
import { backend } from "../../config";
import { logOut } from "../../helpers";

const PostWindow = ({post, isVertical, refreshPost}) =>
{
	const history = useHistory();
	const location = useLocation();
	const addComment = useRef();
	const dispatch = useDispatch();
	const currentUser = useSelector(state => state.currentUser);

	const createdAtDate = new Date(post.createdAt);
	const [fullCommentsFlag, setFullCommentsFlag] = useState(false);

	const [isInfoValid, setIsInfoValid] = useState(false);
	const [commentLoading, setCommentLoading] = useState(false);
	const [isDialogBoxOpen, setIsDialogBoxOpen] = useState(false);
	const closeDialogBox = () => setIsDialogBoxOpen(false);
	const [likesWindow, setLikesWindow] = useState(null);
	const [isLoading, setIsLoading] = useState(false);

	useEffect(() =>
	{
		return () => closeDialogBox();
	}, []);

	const handleAddComment = async e =>
	{
		e.preventDefault();
		if (!isInfoValid) return;

		try
		{
			setCommentLoading(true);
			const comment = addComment.current.value;
			if (comment.trim().length < 1)
				return dispatch(setSnackbar("Please enter a comment.", "error"));
			if (comment.length > 2200)
				return dispatch(setSnackbar("Comment too long. max is 2200 characters.", "error"));

			const result = await fetch(`${backend}/comment/post/${post.id}`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
					Authorization: `Bearer ${localStorage.token}`
				},
				body: JSON.stringify({
					comment: comment
				})
			});
			if (result.status === 401)
				return logOut(dispatch, history);

			if (!result.ok)
			{
				const resultJSON = await result.json();
				throw new Error(resultJSON.detail, { cause: resultJSON.errors });
			}
			await refreshPost();
			setCommentLoading(false);
			addComment.current.value = "";
		} catch (err)
		{
			dispatch(setSnackbar(err.message ?? "Oops, try again later.", "error"));
		}
	};

	const handleTextareaEnter = e =>
	{
		if(e.keyCode === 13 && e.shiftKey === false)
		{
			e.preventDefault();
			handleAddComment(e);
		}
	};

	const deletePost = async () =>
	{
		try
		{
			if (currentUser.username !== post.user.username) throw new Error("Fuck off");
			setIsLoading(true);
			
			const result = await fetch(`${backend}/post/delete/${post.id}`, {
				method: "POST",
				headers: {
					Authorization: `Bearer ${localStorage.token}`
				}
			});
			if (result.status === 401)
				return logOut(dispatch, history);

			if (!result.ok) {
				const resultJSON = await result.json();
				throw new Error(resultJSON.detail, { cause: resultJSON.errors });
			}
			history.push("/");
		} catch (err)
		{
			dispatch(setSnackbar(err.message, "error"));
		}
		setIsLoading(false);
	};

	if (!currentUser || !post) return null;
	if (isVertical)
		return(
			<div className="post-window outlined vertical">
				{ likesWindow
					? <FollowWindow title="Likes" users={likesWindow} closeFollowListWindow={() => setLikesWindow(null)}/>
					: null
				}
				{ isDialogBoxOpen &&
				<div className="dialog-box-container" onClick={closeDialogBox}>
					<div className="dialog-box" onClick={e => e.stopPropagation()}>
						{location.pathname !== "/" ? null : <button className="text" onClick={() => history.push(`/p/${post.id}`)}>Go to post</button>}
						{location.pathname !== "/" ? null : <button className="text" onClick={() => { navigator.clipboard.writeText(`${window.location.origin}/#/p/${post.id}`); closeDialogBox(); }}>Copy Link</button>}
						{currentUser.user.uid === post.user && <button className="remove text" onClick={deletePost}>Delete</button>}
						<button className="cancel text" onClick={closeDialogBox}>Cancel</button>
					</div>
				</div>
				}
				<div className="poster">
					<Link to={`/${post.user.username}`}><div className="profile-pic outlined round"><img src={post.user.profilePic} alt="Profile Pic"></img></div></Link>
					<Link to={`/${post.user.username}`} className="username">{post.user.username}</Link>
					<button className="icon" style={{width: 15, height: 15}} onClick={() => setIsDialogBoxOpen(true)}><svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24"><path d="M6 12c0 1.657-1.343 3-3 3s-3-1.343-3-3 1.343-3 3-3 3 1.343 3 3zm9 0c0 1.657-1.343 3-3 3s-3-1.343-3-3 1.343-3 3-3 3 1.343 3 3zm9 0c0 1.657-1.343 3-3 3s-3-1.343-3-3 1.343-3 3-3 3 1.343 3 3z"/></svg></button>
				</div>
				<div className="post-photo">
					<div className="container">
						<img src={post.photo} alt={post.caption}></img>
					</div>
				</div>
				<div className="side">
					<div className="info">
						<div className="interactions">
							<div style={{display: "flex"}}>
								<Like target={post}/>
								<button className="comment-bubble icon"><svg width="25" height="25" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" fillRule="evenodd" clipRule="evenodd"><path d="M12 1c-6.338 0-12 4.226-12 10.007 0 2.05.739 4.063 2.047 5.625l-1.993 6.368 6.946-3c1.705.439 3.334.641 4.864.641 7.174 0 12.136-4.439 12.136-9.634 0-5.812-5.701-10.007-12-10.007m0 1c6.065 0 11 4.041 11 9.007 0 4.922-4.787 8.634-11.136 8.634-1.881 0-3.401-.299-4.946-.695l-5.258 2.271 1.505-4.808c-1.308-1.564-2.165-3.128-2.165-5.402 0-4.966 4.935-9.007 11-9.007"/></svg></button>
							</div>
							<Save target={post}/>
						</div>
						<span className="likes" onClick={() => setLikesWindow(post.likes)}><span className="number">{post.likes.length}</span> likes</span>
					</div>
					<div className="comments">
						{ post.caption?.length === 0 ||
							<Comment
								commentData={{user: post.user, comment: post.caption, timestamp: post.timestamp}}
								noLike
								noPhoto
								noTimestamp
								noLikesCounter
								noRemove
								postData={post}
							/>
						}
						{post.comments.length > 2 && !fullCommentsFlag &&
							<Link onClick={() => setFullCommentsFlag(true)} to={`/p/${post.id}`} style={{color: "#8e8e8e", fontSize: "0.9em", fontWeight: "500"}}>View all {post.comments.length} comments</Link>}
						{
							post.comments.slice(0, fullCommentsFlag ? post.comments.length : 2).map(comment =>
								<Comment
									postData = {post}
									commentData={comment}
									key={comment.id}
									setLikesWindow={setLikesWindow}
									refreshComments={refreshPost}
									noPhoto
									noTimestamp
									noLikesCounter
								/>)
						}
						<span className="timestamp" title={format(createdAtDate, "d MMM, yyyy")}>
							{
								(new Date() - createdAtDate) / (1000 * 60 * 60 * 24) > 7
									?	getYear(createdAtDate) === getYear(new Date())
										? format(createdAtDate, "d MMMM")
										: format(createdAtDate, "d MMMM, yyyy")
									: formatDistanceToNowStrict(createdAtDate)
							}
						</span>
					</div>
					<form className="add-comment-container" onSubmit={handleAddComment}>
						<textarea className={commentLoading ? "add-comment disabled" : "add-comment"} placeholder="Add a comment..." ref={addComment} onChange={() => addComment.current.value.length > 0 ? setIsInfoValid(true) : setIsInfoValid(false)} onKeyDown={handleTextareaEnter}/>
						<button className={`${isInfoValid ? "text" : "text disabled"}`}>Post</button>
					</form>
				</div>
			</div>
		);
	else return(
		<div className={`post-window outlined ${isLoading ? "disabled" : ""}`}>
			{ likesWindow
				? <FollowWindow title="Likes" users={likesWindow} closeFollowListWindow={() => setLikesWindow(null)}/>
				: null
			}
			{ isDialogBoxOpen &&
				<div className="dialog-box-container" onClick={closeDialogBox}>
					<div className="dialog-box" onClick={e => e.stopPropagation()}>
						{currentUser && currentUser.username === post.user.username && <button className="remove text" onClick={deletePost}>Delete</button>}
						<button className="cancel text" onClick={closeDialogBox}>Cancel</button>
					</div>
				</div>
			}
			<img src={post.photo} alt={post.caption}></img>
			<div className="side">
				<div className="poster">
					<Link to={`/${post.user.username}`}><div className="profile-pic outlined round"><img src={post.user.profilePic} alt="Profile Pic"></img></div></Link>
					<Link to={`/${post.user.username}`} className="username">{post.user.username}</Link>
					<VerifiedTick size={15} user={post.user} marginLeft={7.5}/>
					{currentUser && currentUser.username === post.user.username && <button className="icon" style={{width: 15, height: 15}} onClick={() => setIsDialogBoxOpen(true)}><svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24"><path d="M6 12c0 1.657-1.343 3-3 3s-3-1.343-3-3 1.343-3 3-3 3 1.343 3 3zm9 0c0 1.657-1.343 3-3 3s-3-1.343-3-3 1.343-3 3-3 3 1.343 3 3zm9 0c0 1.657-1.343 3-3 3s-3-1.343-3-3 1.343-3 3-3 3 1.343 3 3z"/></svg></button>}
					{currentUser && currentUser.username === post.user.username ? null : <FollowButton target={post.user}></FollowButton>}
				</div>
				<div className="comments">
					{
						<Comment
							commentData={{user: post.user, comment: post.caption, createdAt: post.createdAt}}
							noLike
							noLikesCounter
							noRemove
							postData={post}
						/>
					}
					{
						post.comments.map(comment =>
							<Comment
								commentData={comment}
								key={comment.id}
								setLikesWindow={setLikesWindow}
								postData={post}
								refreshComments={refreshPost}
							/>)
					}
				</div>
				<div className="info">
					<div className="interactions">
						<div style={{display: "flex"}}>
							<Like target={post}/>
							<button className="comment-bubble icon"><svg width="25" height="25" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" fillRule="evenodd" clipRule="evenodd"><path d="M12 1c-6.338 0-12 4.226-12 10.007 0 2.05.739 4.063 2.047 5.625l-1.993 6.368 6.946-3c1.705.439 3.334.641 4.864.641 7.174 0 12.136-4.439 12.136-9.634 0-5.812-5.701-10.007-12-10.007m0 1c6.065 0 11 4.041 11 9.007 0 4.922-4.787 8.634-11.136 8.634-1.881 0-3.401-.299-4.946-.695l-5.258 2.271 1.505-4.808c-1.308-1.564-2.165-3.128-2.165-5.402 0-4.966 4.935-9.007 11-9.007"/></svg></button>
						</div>
						<Save target={post}/>
					</div>
					<span className="likes" onClick={() => setLikesWindow(post.likes)}><span className="number">{post.likes.length}</span> likes</span>
					<span className="timestamp" title={format(createdAtDate, "d MMM, yyyy")}>
						{
							getYear(createdAtDate) === getYear(new Date())
								? format(createdAtDate, "d MMMM")
								: format(createdAtDate, "d MMMM, yyyy")
						}
					</span>
				</div>
				<form className="add-comment-container" onSubmit={handleAddComment}>
					<textarea className={commentLoading ? "add-comment disabled" : "add-comment"} placeholder="Add a comment..." ref={addComment} onChange={() => addComment.current.value.length > 0 ? setIsInfoValid(true) : setIsInfoValid(false)} onKeyDown={handleTextareaEnter}/>
					<button className={`${isInfoValid ? "text" : "text disabled"}`}>Post</button>
				</form>
			</div>
		</div>
	);
};

export default PostWindow;
