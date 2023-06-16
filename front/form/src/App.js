import React, { useState} from "react";
import './styles/head.css';
import Edit from "./components/Edit";
import CalculationObj from "./components/Calculation";

let next_id = 0;

function App() {
  const [structForDeliveryRequest, setStructForDeliveryRequest] = useState({
    user_id : Number, 
    goods : []
  });

  const [user_id_form1, setUserIdForm1] = useState('');
  const [user_id_form2, setUserIdForm2] = useState('');
  const [count_goods_take, setCountGoodsTake] = useState('');
  const [count_goods_skip, setCountGoodsSkip] = useState('');

  const [good, setGood] = useState({
    height : '',
    length : '',
    width : '',
    weight : '',
  });

  const [count_goods, setCountGoods] = useState(0);

  const [delivery_result, setDeliveryResult] = useState('');

  const [arrCalculations, setArrCalculations] = useState([]);

  const addGood = (e) => {
    e.preventDefault();
    if (user_id_form1 !== '' && good.weight !== '' && good.width !== '' && good.length !== '' && good.height !== '' &&
        !isNaN(user_id_form1) && !isNaN(good.weight) && !isNaN(good.width) && !isNaN(good.length) && !isNaN(good.height)) {
      setDeliveryResult('');
      setStructForDeliveryRequest(
        {
          user_id : user_id_form1,
          goods : [...structForDeliveryRequest.goods, good]
        });
      setCountGoods(count_goods + 1);
      setGood({ height : '', length : '', width : '', weight : ''});
    }
  }

  const DeliveryPrice = async () => {
    let response = await fetch('http://localhost:5001/v1/delivery-prices/calculate', 
    {
      method : 'POST',
      headers : {
        'Content-Type': 'application/json;charset=utf-8',
        'Access-Control-Allow-Origin' : 'http://localhost:3000'
      },
      body : JSON.stringify(structForDeliveryRequest)
    });
    if (response.ok) {
      let res = await response.json();
      setDeliveryResult(res.price);
      setUserIdForm1('');
      setStructForDeliveryRequest({user_id : '', goods : []});
      setCountGoods(0);
    }
    else {
      setDeliveryResult('');
    }
  }

  const GetCalculations = async () => {
    let response = await fetch('http://localhost:5001/v1/delivery-prices/get-history',
    {
      method : 'POST',
      headers : {
        'Content-Type': 'application/json;charset=utf-8',
        'Access-Control-Allow-Origin' : 'http://localhost:3000'
      },
      body : JSON.stringify({user_id : user_id_form2, take : count_goods_take, skip : count_goods_skip})
    });
    if (response.ok) {
      setArrCalculations([]);
      let arr = await response.json();
      let arrCalc = [];
      for (var index = 0; index < arr.length; ++index)
      {
        arrCalc = [
          ...arrCalc,
          {
            volume : arr[index].cargo.volume, 
            weight : arr[index].cargo.weight, 
            good_ids : arr[index].cargo.good_ids,
            price : arr[index].price
          }];
      }
      setArrCalculations(arrCalc);
    }
  }

  const ClearHistory = () => {
    setArrCalculations([]);
  }

  return (
    <div className="App">
        <div className="headDiv">
          <p className="title">
            Price calculator service
          </p>
          <p className="signature">
            Designed by Badamshin Marat
          </p>
        </div>

        <div className="bodyDiv">
          <div className="form1">
            <p className="form_title">
              Подсчет стоимости доставки для товаров:
            </p>
            <div className="elements_container">
                <div className="components_head_form1">
                  <Edit post = {{text : "Укажите USER_ID:", name : "User_Id_form1"}} value={user_id_form1} 
                  onChange={e => setUserIdForm1(e.target.value)}/>
                </div>

                <div className="components_body_form1">
                  <p align="center" className="goods_body_form1">
                    Параметры товара:
                  </p>
                  
                  <div className="components_elements_form1">
                    <div className="component_left_form1">
                      <Edit post = {{text : "Высота:", name : "height"}} value={good.height} onChange={e => setGood(
                        {
                          height : e.target.value,
                          length : good.length,
                          width : good.width,
                          weight : good.weight
                        })}/>
                    </div>
                    <div className="component_right_form1">
                      <Edit post = {{text : "Длина:", name : "length"}} value={good.length} onChange={e => setGood(
                        {
                          height : good.height,
                          length : e.target.value,
                          width : good.width,
                          weight : good.weight
                        })}/>
                    </div>
                  </div>
                  <div className="components_elements_form1">
                    <div className="component_left_form1">
                      <Edit post = {{text : "Вес:", name : "weight"}} value={good.weight} onChange={e => setGood(
                        {
                          height : good.height,
                          length : good.length,
                          width : good.width,
                          weight : e.target.value
                        })}/>
                    </div>
                    <div className="component_right_form1">
                      <Edit post = {{text : "Ширина", name : "width"}} value={good.width} onChange={e => setGood(
                        {
                          height : good.height,
                          length : good.length,
                          width : e.target.value,
                          weight : good.weight,
                        })}/>
                    </div>
                  </div>

                  <div className="components_elements_form1">
                    <div className="component_left_form1">
                      <button className="button_add_form1" onClick={addGood}>
                        Добавить товар
                      </button>
                    </div>
                    <div className="component_right_form1">
                      <Edit post = {{text : "Число товаров:", name : "count_goods"}} readOnly value={count_goods} onChange={e => setCountGoods(e.target.value)}/>
                    </div>
                  </div>

                  <div className="component_center_form1">
                    <button className="button_delivery_form1" onClick={DeliveryPrice}>
                      Подсчитать стоимость
                    </button>
                  </div>

                  <div className="component_center_form1">
                    <input type="text" name="price" readOnly className="edit_for_price" value={delivery_result}></input>
                  </div>
                </div>
            </div>
          </div>
          <div className="form2">
              <p className="form_title">
                Получить историю:
              </p>
              <div className="elements_container">
                <div className="component_form2">
                  <Edit post = {{text : "Укажите USER_ID", name : "user_id_form2"}} value={user_id_form2} 
                      onChange={e => setUserIdForm2(e.target.value)}/>
                </div>

                <div className="component_form2">
                  <Edit post = {{text : "Сколько отобразить:", name : "Count_Goods_Take_form2"}} value={count_goods_take}
                      onChange={e => setCountGoodsTake(e.target.value)}/>
                </div>

                <div className="component_form2">
                  <Edit post = {{text : "Сколько нужно пропустить:", name : "Count_Goods_Skip_form2"}} value={count_goods_skip}
                      onChange={e => setCountGoodsSkip(e.target.value)}/>
                </div>

                <div className="component_center_form2">
                  <button className="button_get_history" onClick={GetCalculations}>
                    Получить историю
                  </button>
                </div>
                <div className="component_center_form2">
                  <button className="button_clear" onClick={ClearHistory}>
                    Очистить историю
                  </button>
                </div>
                <div className="component_center_elements" value={arrCalculations}>
                  {
                  arrCalculations.map((element, index) =>
                    <CalculationObj calculation = {element} number = {index + 1} key={++next_id}/>
                  )}
                </div>
              </div>
          </div>  
        </div>
    </div>
  );
}

export default App;