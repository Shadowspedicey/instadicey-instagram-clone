import { useEffect, useRef, useState } from "react";
import { Link, useHistory } from "react-router-dom";
import { useDispatch } from "react-redux";
import { startLoading, stopLoading } from "../../state/actions/isLoading";
import { setUser } from "../../state/actions/currentUser";
import ErrorMsg from "./ErrorMsg";
import nameLogo from "../../assets/namelogo.png";
import emailVerificationIcon from "../../assets/misc/email-verification.png";
import { backend } from "../../config";
import { ping } from "../../helpers";
import { setSnackbar } from "../../state/actions/snackbar";

const SignUpPage = () =>
{
	const emailRef = useRef();
	const realNameRef = useRef();
	const usernameRef = useRef();
	const passwordRef = useRef();

	const history = useHistory();
	const dispatch = useDispatch();

	const [isInfoValid, setInfoValid] = useState(false);
	const [emailVerificationTime, setEmailVerificationTime] = useState(false);
	const [verificationInterval, setVerificationInterval] = useState();
	const [errorMsg, setErrorMsg] = useState(null);

	useEffect(() => document.title = "Sign Up • Instadicey", []);
	useEffect(() => () => clearInterval(verificationInterval));

	const checkEmail = () =>
	{
		const emailValue = emailRef.current.value.trim();
		if (emailValue === "") return false;
		// eslint-disable-next-line no-control-regex
		else if (!emailValue.match(/(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])/))
			return false;
		else return true;
	};

	const checkPassword = () => passwordRef.current.value.length < 6 ? false : true;

	const checkUsername = () => usernameRef.current.value.length > 20 || usernameRef.current.value.length === 0 || !usernameRef.current.value.match(/^[A-Za-z0-9]*$/) ? false : true;

	const checkForm = () =>
	{
		if (checkEmail() && checkPassword() && checkUsername()) setInfoValid(true);
		else setInfoValid(false);
	};

	const handleSubmit = async  e =>
	{
		if (!(await ping()))
		{
			setErrorMsg("Server is down.");
			dispatch(setSnackbar("Server is down.", "error"));
			return;
		}

		e.preventDefault();
		if (!isInfoValid) return;
		setErrorMsg(null);
		
		const email = emailRef.current.value.toLowerCase();
		const realName = realNameRef.current.value;
		const username = usernameRef.current.value.toLowerCase();
		const password = passwordRef.current.value;
		
		try
		{
			dispatch(startLoading());

			const info = { email, username, realName, password };
			const result = await fetch(`${backend}/auth/register`, {
				method: "POST",
				body: JSON.stringify(info),
				headers: {
					"Content-Type": "application/json"
				},
				mode: "cors"
			});
			
			if (!result.ok) {
				const resultBody = await result.json();
				throw new Error(resultBody.detail, {cause: resultBody.errors});
			}
			setEmailVerificationTime(true);
			//runEmailVerification(info);
		} catch (err)
		{
			console.log(err);
			const errors = err.cause ?? [];
			const errorMessages = [];
			for (var i = 0; i < errors.length; i++)
				if (errors[i].code === "DuplicateUserName")
					errorMessages.push("Username already in use.");
				else if (errors[i].code === "DuplicateEmail")
					errorMessages.push("Email already in use.");
			errors.length === 0
				? setErrorMsg("An error has occurred.")
				: setErrorMsg(errorMessages.join("\n"));

		}
		dispatch(stopLoading());
	};

	useEffect(() => dispatch(stopLoading()), [dispatch]);

	// Runs after checking user info on sign up
	const runEmailVerification = (info) =>
	{
		setVerificationInterval(setInterval(async () =>
		{
			// TODO: Check if email was verified
			// if (currentUser.emailVerified)
			// {
			// 	// dispatch(setUser({user: auth.currentUser, info}));
			// 	history.push("");
			// };
		}, 5000));
	};

	if (!emailVerificationTime)
		return(
			<div id="signup-page">
				<div id="signup-window" className="outlined">
					<div className="logo"><img src={nameLogo} alt="Instadicey logo"></img></div>
					{ 
						errorMsg 
							? <ErrorMsg text={errorMsg}/>
							: <span style={{fontWeight: "bold", color: "rgba(50, 50, 50, 0.5)"}}>Sign up now!</span> 
					}
					<form className="info" onSubmit={handleSubmit}>
						<input type="text" placeholder="Email" id="email" ref={emailRef} onChange={checkForm}></input>
						<input type="text" placeholder="Full Name" id="name" ref={realNameRef}></input>
						<input type="text" placeholder="Username" id="username" ref={usernameRef} onChange={checkForm}></input>
						<input type="password" placeholder="Password" id="password" ref={passwordRef} onChange={checkForm}></input>
						<button id="signup" className={`${isInfoValid ? null : "disabled"}`}>Sign Up</button>
					</form>
				</div>
				<div className="extra outlined"><span>Have an account? <Link to="/" className="button">Log In</Link></span></div>
			</div>
		);
	else
		return(
			<div id="email-verification-window" className="outlined">
				<div className="icon"><img src={emailVerificationIcon} alt="email verification icon"></img></div>
				<p className="header">A verification link has been sent to your email</p>
				<p>Please click on the link sent to your email address and come back here to continue.</p>
			</div>
		);
};

export default SignUpPage;
