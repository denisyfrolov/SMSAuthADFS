$URI = "http://localhost:8080/1/smsmessaging/outbound/2260/requests"
$phoneNumber = "+79111234567"
$senderName = "TestSenderName"
$text = "Hi denis@contoso.local. Your OTP code is 83704"
 
$userName = "TestUserName"
$plainPassword  = "TestPassword"
$pair = "${userName}:${plainPassword}"
$bytes = [System.Text.Encoding]::ASCII.GetBytes($pair)
$base64 = [System.Convert]::ToBase64String($bytes)
$basicAuthValue = "Basic $base64"
$headers = @{ Authorization = $basicAuthValue }
 
$enc = [System.Text.Encoding]::UTF8
$postData = @"
{"address":["tel:$phoneNumber"],"senderAddress":"$senderName","outboundSMSTextMessage":{"message":"$text"}}
"@
 
 
$out = Invoke-WebRequest -Uri $URI -Method POST -Body ( $enc.GetBytes($postData)) -ContentType "application/json" -Headers $headers -ErrorVariable "myError" 
