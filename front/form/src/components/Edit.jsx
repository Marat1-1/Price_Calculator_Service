import React from "react";
import '../styles/head.css';

const Edit = (props) => {
    return (
        <div>
            <p className="components_title">
                {props.post.text}
            </p>
            <input type="text" name={props.post.name} className="edit" {...props}></input>
        </div>
    );
}

export default Edit;