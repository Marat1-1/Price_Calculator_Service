import React from "react";
import '../styles/calculation.model.css'


const CalculationObj = (props) => {
    let ids = '';
    for (var i = 0; i < props.calculation.good_ids.length; ++i) {
        ids += props.calculation.good_ids[i] + ' ';
    }
    return (
        <div className="calculation_model">
            <p className="calculation_model_font_style_bold">Параметры заказа:</p><br/>
            <p className="calculation_model_font_style">
                Номер в списке: {props.number}<br/>
                Объём заказа: {props.calculation.volume}<br/>
                Вес заказа: {props.calculation.weight}<br/>
                Id товаров: {ids}<br/>
                Общая стоимость заказа: {props.calculation.price}
            </p>
        </div>
    );
}

export default CalculationObj;