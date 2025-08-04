import { useRef, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { setSnackbar } from "../../state/actions/snackbar";
import LoadingPage from "../LoadingPage";
import Loading from "../../assets/misc/loading.jpg";
import { backend } from "../../config";
import { logOut, refreshOrLogout } from "../../helpers";
import { useHistory } from "react-router-dom/cjs/react-router-dom.min";

const EditProfile = () =>
{
	const dispatch = useDispatch();
	const history = useHistory();
	const currentUser = useSelector(state => state.currentUser);
	const [isInfoValid, setIsInfoValid] = useState(false);
	const [isEmailValid, setIsEmailValid] = useState(false);
	const [isLoadingInfo, setIsLoadingInfo] = useState(false);
	const [isLoadingEmail, setIsLoadingEmail] = useState(false);
	const [isPhotoLoading, setIsPhotoLoading] = useState(false);
	const [isPhotoChangerBoxOpen, setIsPhotoChangerBoxOpen] = useState(false);

	const uploadRef = useRef();
	const nameRef = useRef();
	const usernameRef = useRef();
	const bioRef = useRef();
	const emailRef = useRef();
	const confirmPasswordRef = useRef();

	const openPhotoBox = () => setIsPhotoChangerBoxOpen(true);
	const closePhotoBox = () => setIsPhotoChangerBoxOpen(false);
	const removePhoto = async () =>
	{
		closePhotoBox();
		setIsPhotoLoading(true);
		try {
			const result = await fetch(`${backend}/user/edit/profile-pic/reset`, {
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
			await updateUserLocally();
		} catch(err) {
			dispatch(setSnackbar(err.message, "error"));
		}
		setIsPhotoLoading(false);
	};
	const openPhotoUpload = () => uploadRef.current.click();
	const uploadPhoto = async input =>
	{
		try
		{
			const uploadedPhoto = input.target.files[0];
			if (!uploadedPhoto)
				throw new Error("No photo uploaded");
			const acceptedFormats = uploadRef.current.accept.split(", ");
			if (!acceptedFormats.includes(uploadedPhoto.type))
			  throw new Error("Not a supported photo format");
			closePhotoBox();
			setIsPhotoLoading(true);

			const form = new FormData();
			form.append("newProfilePic", uploadedPhoto);
			const result = await fetch(`${backend}/user/edit/profile-pic`, {
				method: "POST",
				headers: {
					Authorization: `Bearer ${localStorage.token}`
				},
				body: form
			});
			if (result.status === 401)
				return logOut(dispatch, history);
			if (!result.ok)
			{
				const resultJSON = await result.json();
				throw new Error(resultJSON.detail, { cause: resultJSON.errors });
			}
	
			await updateUserLocally();
			uploadRef.current.value = "";
			setIsPhotoLoading(false);
			dispatch(setSnackbar("Photo updated successfully", "success"));
		} catch (err)
		{
			console.error(err);
			if (err.message === "No photo uploaded")
				return null;
			if (err.message === "Not a supported photo format")
				return dispatch(setSnackbar("Not a supported photo format.", "error"));
			dispatch(setSnackbar("Oops, please try again later.", "error"));
		}
	};

	const handleChange = () => isInfoValid ? null : setIsInfoValid(true);

	const handleEmailChange = async e =>
	{
		e.preventDefault();
		if (!isEmailValid) return;
		setIsLoadingEmail(true);
		const email = emailRef.current.value;
		const password = confirmPasswordRef.current.value;
		try {
			if (email.trim() === "") throw new Error("Email can't be empty");
			// eslint-disable-next-line no-control-regex
			if (!email.match(/(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])/))
				throw new Error("Email is invalid");
			if (password.trim() === "") throw new Error("Password can't be empty");
			
			const result = await fetch(`${backend}/auth/send-email-change-verification`, {
				method: "POST",
				headers: {
					Authorization: `Bearer ${localStorage.token}`,
					"Content-Type": "application/json"
				},
				body: JSON.stringify({
					email,
					password
				})
			});
			if (result.status === 401)
				return logOut(dispatch, history);
			if (!result.ok) {
				const resultJSON = await result.json();
				throw new Error(resultJSON.detail);
			}
			dispatch(setSnackbar("Email verification sent.", "success"));
		} catch(err) {
			dispatch(setSnackbar(err.message, "error"));
		}
		setIsLoadingEmail(false);
	};

	const handleSubmit = async e =>
	{
		e.preventDefault();
		if (!isInfoValid) return;
		setIsLoadingInfo(true);
		try
		{
			let realName = nameRef.current.value;
			let username = currentUser.username === "guest" ? "guest" : usernameRef.current.value;
			let bio = bioRef.current.value;
			
			if (currentUser.username !== "guest" && currentUser.username !== usernameRef.current.value) {
				if (username.length > 20) throw new Error("Username too long");
				if (username.trim() === "") throw new Error("Username not entered");
				if (!username.match(/^[A-Za-z0-9]*$/)) throw new Error("Username not English");
			}
			if (currentUser.bio !== bioRef.current.value)
				if (bio.length > 150) throw new Error("Bio too long");

			const result = await fetch(`${backend}/user/edit`, {
				method: "POST",
				headers: {
					Authorization: `Bearer ${localStorage.token}`,
					"Content-Type": "application/json"
				},
				body: JSON.stringify({
					username: username,
					realname: realName.trim().length === 0 ? null : realName,
					bio: bio.trim().length === 0 ? null : bio
				})
			});
			if (result.status === 401)
				return logOut(dispatch, history);
			if (!result.ok)
			{
				const resultJSON = await result.json();
				throw new Error(resultJSON.detail, { cause: resultJSON.errors });
			}
			await updateUserLocally();
			dispatch(setSnackbar("Info updated.", "success"));
		} catch (err)
		{
			setIsInfoValid(false);
			dispatch(setSnackbar(err.message ?? "Oops, please try again later.", "error"));
		}
		setIsLoadingInfo(false);
	};

	const updateUserLocally = async () => await refreshOrLogout(dispatch, history);

	if (!currentUser) return <LoadingPage/>;
	return(
		<div className="account-element edit-profile">
			{ isPhotoChangerBoxOpen &&
					<div className="dialog-box-container" onClick={closePhotoBox}>
						<div className="dialog-box" onClick={e => e.stopPropagation()}>
							<h2>Change Profile Photo</h2>
							<button className="upload text" onClick={openPhotoUpload}>Upload Photo</button>
							<button className="remove text" onClick={removePhoto}>Remove Current Photo</button>
							<button className="cancel text" onClick={closePhotoBox}>Cancel</button>
						</div>
					</div>
			}
			<div className="element profile-pic-container">
				<div className="img-container outlined left" onClick={openPhotoBox}>
					{ isPhotoLoading && <div className="loading"><img src={Loading} alt="loading"></img></div>}
					<img src={currentUser.profilePic} alt={`${currentUser.username}'s profile pic'`}></img>
					<input id="photo" type="file" accept="image/png, image/jpg, image/jpeg, image/pjpeg, image/jfif, image/pjp" style={{display: "none"}} ref={uploadRef} onChange={uploadPhoto}></input>
				</div>
				<div className="right">
					{currentUser.username}
					<button className="text" onClick={openPhotoBox}>Change Profile Photo</button>
				</div>
			</div>
			<form onSubmit={handleSubmit}>
				<div className="element">
					<label htmlFor="name" className="left">Name</label>
					<div className="right">
						<input type="text" id="name" defaultValue={currentUser.realName} placeholder="Name" ref={nameRef} className="outlined" onChange={handleChange}></input>
						<p>Help people discover your account by using the name you're known by: either your full name, nickname, or business name.</p>
					</div>
				</div>
				{ currentUser.username !== "guest" &&
					<div className="element">
						<label htmlFor="username" className="left">Username</label>
						<div className="right">
							<input type="text" id="username" defaultValue={currentUser.username} placeholder="Username" ref={usernameRef} className="outlined" onChange={handleChange}></input>
						</div>
					</div>
				}
				<div className="element">
					<label htmlFor="bio" className="left">Bio</label>
					<div className="right">
						<textarea id="bio" defaultValue={currentUser.bio} ref={bioRef} className="outlined" onChange={handleChange}></textarea>
					</div>
				</div>
				<div className="element">
					<div className="left"></div>
					{
						isLoadingInfo
							? <button className="loading"><div><img src={Loading} alt="loading"></img></div></button>
							: <button className={`submit ${isInfoValid ? null : "disabled"}`}>Submit</button>
					}
				</div>
			</form>
			{ currentUser.username !== "guest" &&
				<form onSubmit={handleEmailChange}>
					<div className="element info">
						<span className="left"></span>
						<div className="right">
							<p className="header">Personal Information</p>
							<p>Provide your personal information, even if the account is used for a business, a pet or something else. This won't be a part of your public profile.</p>
						</div>
					</div>
					<div className="element">
						<label htmlFor="email" className="left">Email</label>
						<div className="right">
							<input type="text" id="email" defaultValue={currentUser.email} placeholder="Email" ref={emailRef} className="outlined" onChange={() => isEmailValid ? null : setIsEmailValid(true)}></input>
							{ !emailRef.current || currentUser.email === emailRef.current.value
								? null 
								: <input type="password" id="password" placeholder="Confirm Password" ref={confirmPasswordRef} className="outlined"></input>
							}
						</div>
					</div>
					<div className="element">
						<div className="left"></div>
						{
							isLoadingEmail
								? <button className="loading"><div><img src={Loading} alt="loading"></img></div></button>
								: <button className={`submit ${isEmailValid ? null : "disabled"}`}>Submit</button>
						}
					</div>
				</form>
			}
		</div>
	);
};

export default EditProfile;
